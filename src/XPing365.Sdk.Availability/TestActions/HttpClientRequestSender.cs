using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Configurations;
using XPing365.Sdk.Core.DependencyInjection;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Availability.TestActions;

/// <summary>
/// The HttpClientRequestSender class is a concrete implementation of the <see cref="TestComponent"/> class that is used 
/// to send an HTTP request. It uses the <see cref="IHttpClientFactory"/> to create an instance of the 
/// <see cref="HttpClient"/> class, which is used to send the HTTP request.
/// </summary>
/// <remarks>
/// Before using this test component, you need to register the necessary services by calling the 
/// <see cref="DependencyInjectionExtension.AddHttpClients(IServiceCollection, Action{IServiceProvider, HttpClientConfiguration}?)(IServiceCollection)"/>
/// method.
public sealed class HttpClientRequestSender() : TestComponent(name: StepName, type: TestStepType.ActionStep)
{
    public const string StepName = "Send HTTP Request";

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
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        IHttpClientFactory httpClientFactory =
            serviceProvider.GetService<IHttpClientFactory>() ??
            throw new InvalidProgramException(Errors.HttpClientsNotFound);

        using HttpClient httpClient = CreateHttpClient(settings, httpClientFactory);
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

            byte[] buffer = await ReadAsByteArrayAsync(response.Content, cancellationToken).ConfigureAwait(false);
            testStep = context.SessionBuilder
                .Build(PropertyBagKeys.HttpStatus, new PropertyBagValue<string>($"{response.StatusCode}"))
                .Build(PropertyBagKeys.HttpVersion, new PropertyBagValue<string>($"{response.Version}"))
                .Build(PropertyBagKeys.HttpReasonPhrase, new PropertyBagValue<string?>(response.ReasonPhrase))
                .Build(PropertyBagKeys.HttpResponseHeaders, GetHeaders(response.Headers))
                .Build(PropertyBagKeys.HttpResponseTrailingHeaders, GetHeaders(response.TrailingHeaders))
                .Build(PropertyBagKeys.HttpContentHeaders, GetHeaders(response.Content.Headers))
                .Build(PropertyBagKeys.HttpContent, new PropertyBagValue<byte[]>(buffer))
                .Build(PropertyBagKeys.HttpResponseMessage, new NonSerializable<HttpResponseMessage>(response))
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

    private static async Task<byte[]> ReadAsByteArrayAsync(HttpContent httpContent, CancellationToken cancellationToken)
    {
        // When storing the server response content, it is generally recommended to store it as a byte array
        // rather than a string. This is because the response content may contain binary data that cannot be represented
        // as a string.

        // If you need to convert the byte array to a string for display purposes, you can use the Encoding class to
        // specify the character encoding to use.
        return await httpContent.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    private static HttpClient CreateHttpClient(TestSettings settings, IHttpClientFactory httpClientFactory)
    {
        HttpClient httpClient = null!;
        if (settings.RetryHttpRequestWhenFailed == true && settings.FollowHttpRedirectionResponses == true)
        {
            httpClient = httpClientFactory.CreateClient(
                HttpClientConfiguration.HttpClientWithRetryAndFollowRedirect);
        }
        else if (settings.RetryHttpRequestWhenFailed == true && settings.FollowHttpRedirectionResponses == false)
        {
            httpClient = httpClientFactory.CreateClient(
                HttpClientConfiguration.HttpClientWithRetryAndNoFollowRedirect);
        }
        else if (settings.RetryHttpRequestWhenFailed == false && settings.FollowHttpRedirectionResponses == false)
        {
            httpClient = httpClientFactory.CreateClient(
                HttpClientConfiguration.HttpClientWithNoRetryAndNoFollowRedirect);
        }
        else if (settings.RetryHttpRequestWhenFailed == false && settings.FollowHttpRedirectionResponses == true)
        {
            httpClient = httpClientFactory.CreateClient(
                HttpClientConfiguration.HttpClientWithNoRetryAndFollowRedirect);
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
