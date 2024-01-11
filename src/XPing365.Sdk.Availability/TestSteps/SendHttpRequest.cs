using XPing365.Sdk.Availability.Extensions;
using XPing365.Sdk.Core;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestSteps;

internal sealed class SendHttpRequest(IHttpClientFactory httpClientFactory) : 
    TestStepHandler(StepName, TestStepType.ActionStep)
{
    public const string StepName = "Sent HTTP Request";

    internal const string HttpClientWithNoRetryAndFollowRedirect = nameof(HttpClientWithNoRetryAndFollowRedirect);
    internal const string HttpClientWithNoRetryAndNoFollowRedirect = nameof(HttpClientWithNoRetryAndNoFollowRedirect);
    internal const string HttpClientWithRetryAndNoFollowRedirect = nameof(HttpClientWithRetryAndNoFollowRedirect);
    internal const string HttpClientWithRetryAndFollowRedirect = nameof(HttpClientWithRetryAndFollowRedirect);

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public override async Task<TestStep> HandleStepAsync(
        Uri uri,
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = CreateHttpClient(settings);
        using HttpRequestMessage request = CreateHttpRequestMessage(uri, settings);

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
            Method = settings.GetHttpMethodOrDefault(),
            Content = settings.GetHttpContentOrDefault()
        };
    }
}
 