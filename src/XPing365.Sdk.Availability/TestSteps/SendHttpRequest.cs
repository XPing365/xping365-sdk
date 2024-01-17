using XPing365.Sdk.Availability.Extensions;
using XPing365.Sdk.Core;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestSteps;

/// <summary>
/// The SendHttpRequest class is a concrete implementation of the <see cref="TestStepHandler"/> class that is used to 
/// send an HTTP request. It uses the <see cref="IHttpClientFactory"/> to create an instance of the 
/// <see cref="HttpClient"/> class, which is used to send the HTTP request.
/// </summary>
/// <param name="httpClientFactory"><see cref="IHttpClientFactory"/> implementation instance.</param>
public sealed class SendHttpRequest(IHttpClientFactory httpClientFactory) : 
    TestStepHandler(StepName, TestStepType.ActionStep)
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
    /// <param name="session">A <see cref="TestSession"/> object that represents the test session.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    /// <returns><see cref="TestStep"/> object.</returns>
    public override async Task<TestStep> HandleStepAsync(
        Uri url,
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        HttpClient httpClient = CreateHttpClient(settings);
        using HttpRequestMessage request = CreateHttpRequestMessage(url, settings);

        foreach (var httpHeader in settings.GetHttpRequestHeadersOrEmpty())
        {
            request.Headers.Add(httpHeader.Key, httpHeader.Value);
        }

        using var inst = new InstrumentationLog(startStopwatch: true);

        TestStep testStep = null!;
        try
        {
            HttpResponseMessage response = await httpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            var propertyBag = new PropertyBag(response.ToProperties());
            testStep = CreateSuccessTestStep(inst.StartTime, inst.ElapsedTime, propertyBag);
        }
        catch (Exception exception)
        {
            testStep = CreateTestStepFromException(exception, inst.StartTime, inst.ElapsedTime);
        }

        return testStep;
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

    private static HttpRequestMessage CreateHttpRequestMessage(Uri uri, TestSettings settings)
    {
        return new HttpRequestMessage()
        {
            RequestUri = uri,
            Method = settings.GetHttpMethod(),
            Content = settings.GetHttpContent()
        };
    }
}
 