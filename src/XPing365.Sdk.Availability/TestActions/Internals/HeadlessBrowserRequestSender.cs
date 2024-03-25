using Microsoft.Net.Http.Headers;
using Microsoft.Playwright;
using XPing365.Sdk.Core.HeadlessBrowser;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net;

namespace XPing365.Sdk.Availability.TestActions.Internals;

/// <summary>
/// The HeadlessBrowserRequestSender class is a subclass of the TestComponent abstract class that implements the 
/// ITestComponent interface. It is used to send HTTP requests to a web application using a headless browser, such as 
/// Chromium, Firefox, or WebKit. It uses the Playwright library to create and control the headless browser instance. 
/// </summary>
/// <remarks>
/// Before using this test component, you need to register the necessary services by calling the 
/// <see cref="Core.DependencyInjection.DependencyInjectionExtension.AddBrowserClients(IServiceCollection)"/> method 
/// which adds <see cref="IHeadlessBrowserFactory"/> factory service. The XPing365 SDK provides a default implementation 
/// of this interface, called DefaultHeadlessBrowserFactory, which based on the <see cref="TestSettings"/> creates a 
/// Chromium, WebKit or Firefox headless browser instance. You can also implement your own custom headless browser 
/// factory by implementing the <see cref="IHeadlessBrowserFactory"/> interface and adding its implementation into
/// services.
/// </remarks>
internal sealed class HeadlessBrowserRequestSender(string name) : TestComponent(name, type: TestStepType.ActionStep)
{
    private readonly OrderedHttpRedirections _visitedUrls = [];

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="context">A <see cref="TestContext"/> object that represents the test session.</param>
    /// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    /// <returns><see cref="TestStep"/> object.</returns>
    public override async Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        _visitedUrls.Clear();

        using IHeadlessBrowserFactory? headlessBrowserFactory = 
            serviceProvider.GetService<IHeadlessBrowserFactory>() ?? 
            throw new InvalidProgramException(Errors.HeadlessBrowserNotFound);

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task - impossible to enforce this rule
        await using HeadlessBrowserClient browser = await headlessBrowserFactory
            .CreateClientAsync(settings)
            .ConfigureAwait(false);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        using var instrumentation = new InstrumentationLog(startStopwatch: true);

        void OnHttpRedirection(IResponse response)
        {
            BuildHttpRedirectionStep(url, response, context, settings, instrumentation);
        }

        TestStep testStep = null!;
        try
        {
            _visitedUrls.Add(url.AbsoluteUri);
            WebPage webPage = await browser.GetAsync(url, onHttpRedirection: OnHttpRedirection).ConfigureAwait(false);
            byte[] buffer = await ReadAsByteArrayAsync(
                    webPage.HttpResponseMessage.Content, 
                    cancellationToken)
                .ConfigureAwait(false);
            HttpResponseMessage Response() => webPage.HttpResponseMessage;
            testStep = context.SessionBuilder
                .Build(PropertyBagKeys.HttpStatus, new PropertyBagValue<string>($"{(int)Response().StatusCode}"))
                .Build(PropertyBagKeys.HttpVersion, new PropertyBagValue<string>($"{Response().Version}"))
                .Build(PropertyBagKeys.HttpReasonPhrase, new PropertyBagValue<string?>(Response().ReasonPhrase))
                .Build(PropertyBagKeys.HttpResponseHeaders, GetHeaders(Response().Headers))
                .Build(PropertyBagKeys.HttpResponseTrailingHeaders, GetHeaders(Response().TrailingHeaders))
                .Build(PropertyBagKeys.HttpContentHeaders, GetHeaders(Response().Content.Headers))
                .Build(PropertyBagKeys.HttpContent, new PropertyBagValue<byte[]>(buffer))
                .Build(PropertyBagKeys.HttpResponseMessage, new NonSerializable<HttpResponseMessage>(Response()))
                .Build(component: this, instrumentation);
        }
        catch (Exception exception)
        {
            testStep = context.SessionBuilder.Build(component: this, instrumentation, exception);
        }
        finally
        {
            context.Progress?.Report(testStep);
        }
    }

    private void BuildHttpRedirectionStep(
        Uri url,
        IResponse response,
        TestContext context,
        TestSettings settings,
        InstrumentationLog instrumentation)
    {
        var responseHeadersBag = new PropertyBagValue<Dictionary<string, string>>(
            response.Headers.ToDictionary(
                pair => pair.Key.ToUpperInvariant(),
                pair => pair.Value
        ));

        var httpStatucCode = Enum.Parse<HttpStatusCode>($"{response.Status}", ignoreCase: true);

        var testStep = context.SessionBuilder
            .Build(PropertyBagKeys.HttpStatus, new PropertyBagValue<string>($"{(int)httpStatucCode}"))
            .Build(PropertyBagKeys.HttpReasonPhrase, new PropertyBagValue<string?>(response.StatusText))
            .Build(PropertyBagKeys.HttpResponseHeaders, responseHeadersBag)
            .Build(component: this, instrumentation);
        context.Progress?.Report(testStep);

        // Location HTTP header, specifies the absolute or relative URL of the new resource.
        if (responseHeadersBag.Value.TryGetValue(HeaderNames.Location.ToUpperInvariant(), out string? redirectUrl))
        {
            if (_visitedUrls.Add(redirectUrl) == false)
            {
                // Circular dependency detected
                throw new InvalidOperationException(
                    $"A circular dependency was detected for the URL {redirectUrl}. " +
                    $"The redirection chain is: {string.Join(" -> ", _visitedUrls)}");
            }
        }

        // Throw an exception if the max number of redirects has been reached
        if (_visitedUrls.Count > settings.MaxRedirections)
        {
            throw new TooManyRedirectsException(
                $"The maximum number of redirects ({settings.MaxRedirections}) has been exceeded for the URL " +
                $"{url}. The last redirect URL was " +
                $"{_visitedUrls.FindLastMatchingItem(str => !string.IsNullOrEmpty(str))}.");
        }

        // Restart the instrumentation log after this test step to ensure accurate timing for subsequent steps.
        instrumentation.Restart();
    }

    private static PropertyBagValue<Dictionary<string, string>> GetHeaders(HttpHeaders headers) =>
        new(headers.ToDictionary(h => h.Key.ToUpperInvariant(), h => string.Join(";", h.Value)));

    private static async Task<byte[]> ReadAsByteArrayAsync(HttpContent httpContent, CancellationToken cancellationToken)
    {
        // When storing the server response content, it is generally recommended to store it as a byte array
        // rather than a string. This is because the response content may contain binary data that cannot be represented
        // as a string.

        // If you need to convert the byte array to a string for display purposes, you can use the Encoding class to
        // specify the character encoding to use.
        return await httpContent.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }
}
 