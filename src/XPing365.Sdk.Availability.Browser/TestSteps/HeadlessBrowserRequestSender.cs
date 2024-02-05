using Microsoft.Net.Http.Headers;
using XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;
using XPing365.Sdk.Common;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Availability.TestSteps;

public sealed class HeadlessBrowserRequestSender(IHeadlessBrowserFactory headlessBrowserFactory) :
    TestComponent(StepName, TestStepType.ActionStep)
{
    public const string StepName = "Headless browser request";

    private readonly IHeadlessBrowserFactory _headlessBrowserFactory = headlessBrowserFactory;

    public override async Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using HeadlessBrowserClient browser = await _headlessBrowserFactory
            .CreateClientAsync(CreateBrowserContext(settings))
            .ConfigureAwait(false);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        using var instrumentation = new InstrumentationLog(startStopwatch: true);

        TestStep testStep = null!;
        try
        { 
            WebPage webPage = await browser.GetAsync(url).ConfigureAwait(false);
            context.SessionBuilder.PropertyBag.AddOrUpdateProperty(
                PropertyBagKeys.HttpResponseMessage, webPage.HttpResponseMessage);
            byte[] buffer = await ReadAsByteArrayAsync(
                    webPage.HttpResponseMessage.Content, 
                    cancellationToken)
                .ConfigureAwait(false);
            context.SessionBuilder.PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpContent, buffer);
            testStep = context.SessionBuilder.Build(component: this, instrumentation);
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

    private static BrowserContext CreateBrowserContext(TestSettings settings)
    {
        settings.GetHttpRequestHeadersOrEmpty().TryGetValue(HeaderNames.UserAgent, out var values);

        return new BrowserContext
        {
            Timeout = settings.PropertyBag.GetProperty<TimeSpan>(PropertyBagKeys.HttpRequestTimeout),
            UserAgent = values?.FirstOrDefault()
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
 