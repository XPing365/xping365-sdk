using XPing365.Sdk.Availability.TestSteps.Internals;
using XPing365.Sdk.Core;
using XPing365.Sdk.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Common;

namespace XPing365.Sdk.Availability.TestSteps;

/// <summary>
/// The SendHttpRequest class is a concrete implementation of the <see cref="TestComponent"/> class that is used to 
/// send an HTTP request. It uses the <see cref="IHttpClientFactory"/> to create an instance of the 
/// <see cref="HttpClient"/> class, which is used to send the HTTP request.
/// </summary>
/// <param name="httpClientFactory"><see cref="IHttpClientFactory"/> implementation instance.</param>
public sealed class SendHttpRequest(IHttpClientFactory httpClientFactory) : 
    TestComponent(StepName, TestStepType.ActionStep)
{
    public const string StepName = "Send HTTP Request";

    internal const string HttpClientWithNoRetryAndFollowRedirect = nameof(HttpClientWithNoRetryAndFollowRedirect);
    internal const string HttpClientWithNoRetryAndNoFollowRedirect = nameof(HttpClientWithNoRetryAndNoFollowRedirect);
    internal const string HttpClientWithRetryAndNoFollowRedirect = nameof(HttpClientWithRetryAndNoFollowRedirect);
    internal const string HttpClientWithRetryAndFollowRedirect = nameof(HttpClientWithRetryAndFollowRedirect);

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="context">A <see cref="TestContext"/> object that represents the test session.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    /// <returns><see cref="TestStep"/> object.</returns>
    public override async Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        HttpClient httpClient = CreateHttpClient(settings);
        using HttpRequestMessage request = CreateHttpRequestMessage(url, settings);

        foreach (var httpHeader in settings.GetHttpRequestHeadersOrEmpty())
        {
            request.Headers.Add(httpHeader.Key, httpHeader.Value);
        }

        using var instrumentation = new InstrumentationLog(startStopwatch: true);

        TestStep testStep = null!;
        try
        {
            using HttpResponseMessage response = await httpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);
            context.SessionBuilder.PropertyBag.AddOrUpdateProperties(response.ToProperties());
            byte[] buffer = await ReadAsByteArrayAsync(response.Content, cancellationToken).ConfigureAwait(false);
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

    private static async Task<byte[]> ReadAsByteArrayAsync(HttpContent httpContent, CancellationToken cancellationToken)
    {
        // When storing the server response content, it is generally recommended to store it as a byte array
        // rather than a string. This is because the response content may contain binary data that cannot be represented
        // as a string.

        // If you need to convert the byte array to a string for display purposes, you can use the Encoding class to
        // specify the character encoding to use.
        return await httpContent.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    private HttpClient CreateHttpClient(TestSettings settings)
    {
        HttpClient httpClient = null!;
        if (settings.RetryHttpRequestWhenFailed == true && settings.FollowHttpRedirectionResponses == true)
        {
            httpClient = _httpClientFactory.CreateClient(HttpClientWithRetryAndFollowRedirect);
        }
        else if (settings.RetryHttpRequestWhenFailed == true && settings.FollowHttpRedirectionResponses == false)
        {
            httpClient = _httpClientFactory.CreateClient(HttpClientWithRetryAndNoFollowRedirect);
        }
        else if (settings.RetryHttpRequestWhenFailed == false && settings.FollowHttpRedirectionResponses == false)
        {
            httpClient = _httpClientFactory.CreateClient(HttpClientWithNoRetryAndNoFollowRedirect);
        }
        else if (settings.RetryHttpRequestWhenFailed == false && settings.FollowHttpRedirectionResponses == true)
        {
            httpClient = _httpClientFactory.CreateClient(HttpClientWithNoRetryAndFollowRedirect);
        }

        httpClient.Timeout = settings.PropertyBag.GetProperty<TimeSpan>(PropertyBagKeys.HttpRequestTimeout);

        return httpClient;
    }

    private static HttpRequestMessage CreateHttpRequestMessage(Uri url, TestSettings settings)
    {
        return new HttpRequestMessage()
        {
            RequestUri = url,
            Method = settings.GetHttpMethod(),
            Content = settings.GetHttpContent()
        };
    }
}
 