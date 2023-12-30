using XPing365.Availability.Extensions;
using XPing365.Core;
using XPing365.Core.Extensions;

namespace XPing365.Availability.TestSteps;

internal sealed class SendHttpRequest(IHttpClientFactory httpClientFactory, TestStepHandler? successor) 
    : TestStepHandler(successor)
{
    public const string StepName = "Sent HTTP Request";

    internal const string HttpClientWithNoRetryAndFollowRedirect = nameof(HttpClientWithNoRetryAndFollowRedirect);
    internal const string HttpClientWithNoRetryAndNoFollowRedirect = nameof(HttpClientWithNoRetryAndNoFollowRedirect);
    internal const string HttpClientWithRetryAndNoFollowRedirect = nameof(HttpClientWithRetryAndNoFollowRedirect);
    internal const string HttpClientWithRetryAndFollowRedirect = nameof(HttpClientWithRetryAndFollowRedirect);

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public override async Task HandleStepAsync(
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

        using var instrumentation = new InstrumentationLog(startStopper: true);

        try
        {
            HttpResponseMessage response = await httpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            var propertyBag = new PropertyBag();
            propertyBag.AddOrUpdateProperties(response.ToProperties());

            var testStep = new TestStep(
                Name: StepName,
                StartDate: instrumentation.StartTime,
                Duration: instrumentation.ElapsedTime,
                Type: TestStepType.ActionStep,
                Result: response.IsSuccessStatusCode ? TestStepResult.Succeeded : TestStepResult.Failed,
                PropertyBag: propertyBag,
                ErrorMessage: !response.IsSuccessStatusCode ? response.ReasonPhrase : null);
            session.AddTestStep(testStep);
        }
        catch (HttpRequestException e)
        {
            var testStep = TestStep.CreateActionStepFromException(StepName, e, instrumentation);
            session.AddTestStep(testStep);
        }
        catch (TaskCanceledException e)
        {
            var testStep = TestStep.CreateActionStepFromException(StepName, e, instrumentation);
            session.AddTestStep(testStep);
        }

        await base.HandleStepAsync(uri, settings, session, cancellationToken).ConfigureAwait(false);
    }

    private HttpClient CreateHttpClient(TestSettings settings)
    {
        HttpClient httpClient = null!;

        if (settings.GetHttpRetryOrDefault() == true && settings.GetHttpFollowRedirectOrDefault() == true)
        {
            httpClient = _httpClientFactory.CreateClient(HttpClientWithRetryAndFollowRedirect);
        }
        else if (settings.GetHttpRetryOrDefault() == true && settings.GetHttpFollowRedirectOrDefault() == false)
        {
            httpClient = _httpClientFactory.CreateClient(HttpClientWithRetryAndNoFollowRedirect);
        }
        else if (settings.GetHttpRetryOrDefault() == false && settings.GetHttpFollowRedirectOrDefault() == false)
        {
            httpClient = _httpClientFactory.CreateClient(HttpClientWithNoRetryAndNoFollowRedirect);
        }
        else if (settings.GetHttpRetryOrDefault() == false && settings.GetHttpFollowRedirectOrDefault() == true)
        {
            httpClient = _httpClientFactory.CreateClient(HttpClientWithNoRetryAndFollowRedirect);
        }

        httpClient.Timeout = settings.Timeout;

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
