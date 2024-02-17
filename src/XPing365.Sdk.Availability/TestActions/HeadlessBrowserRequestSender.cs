using Microsoft.Net.Http.Headers;
using Microsoft.Playwright;
using XPing365.Sdk.Core.HeadlessBrowser;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace XPing365.Sdk.Availability.TestActions;

/// <summary>
/// The HeadlessBrowserRequestSender class is a subclass of the TestComponent abstract class that implements the 
/// ITestComponent interface. It is used to send HTTP requests to a web application using a headless browser, such as 
/// Chromium, Firefox, or WebKit. It uses the Playwright library to create and control the headless browser instance. 
/// </summary>
/// <remarks>
/// Before using this test component, you need to register the necessary services by calling the 
/// <see cref="Core.DependencyInjection.DependencyInjectionExtension.AddBrowserClients(IServiceCollection)"/> method 
/// which adds <see cref="IHeadlessBrowserFactory"/> factory service. The XPing365 SDK provides a default implementation 
/// of this interface, called DefaultHeadlessBrowserFactory, which based on the <see cref="BrowserContext"/> creates a 
/// Chromium, WebKit or Firefox headless browser instance. You can also implement your own custom headless browser 
/// factory by implementing the <see cref="IHeadlessBrowserFactory"/> interface and adding its implementation into
/// services.
/// </remarks>
public sealed class HeadlessBrowserRequestSender() : TestComponent(name: StepName, type: TestStepType.ActionStep)
{
    public const string StepName = "Headless browser request";

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

        using IHeadlessBrowserFactory? headlessBrowserFactory = 
            serviceProvider.GetService<IHeadlessBrowserFactory>() ?? 
            throw new InvalidProgramException(Errors.HeadlessBrowserNotFound);

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task - impossible to enforce this rule
        await using HeadlessBrowserClient browser = await headlessBrowserFactory
            .CreateClientAsync(CreateBrowserContext(settings))
            .ConfigureAwait(false);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        using var instrumentation = new InstrumentationLog(startStopwatch: true);

        TestStep testStep = null!;
        try
        { 
            WebPage webPage = await browser.GetAsync(url).ConfigureAwait(false);
            byte[] buffer = await ReadAsByteArrayAsync(
                    webPage.HttpResponseMessage.Content, 
                    cancellationToken)
                .ConfigureAwait(false);
            HttpResponseMessage Response() => webPage.HttpResponseMessage;
            testStep = context.SessionBuilder
                .Build(PropertyBagKeys.HttpStatus, new PropertyBagValue<string>($"{Response().StatusCode}"))
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

    private static PropertyBagValue<Dictionary<string, string>> GetHeaders(HttpHeaders headers) =>
        new(headers.ToDictionary(h => h.Key.ToUpperInvariant(), h => string.Join(";", h.Value)));

    private static BrowserContext CreateBrowserContext(TestSettings settings)
    {
        settings.GetHttpRequestHeadersOrEmpty().TryGetValue(HeaderNames.UserAgent, out var values);

        return new BrowserContext
        {
            Timeout = settings.PropertyBag.GetProperty<TimeSpan>(PropertyBagKeys.HttpRequestTimeout),
            UserAgent = values?.FirstOrDefault(),
            Type = settings.BrowserType,
            ViewportSize = settings.BrowserViewportSize != null ? new ViewportSize
            {
                Height = settings.BrowserViewportSize.Value.Height,
                Width = settings.BrowserViewportSize.Value.Width
            } : null
        };
    }

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
 