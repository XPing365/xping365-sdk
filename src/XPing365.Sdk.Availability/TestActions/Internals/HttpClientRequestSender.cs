using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Configurations;
using XPing365.Sdk.Core.DependencyInjection;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Availability.TestActions.Internals;

/// <summary>
/// The HttpClientRequestSender class is a concrete implementation of the <see cref="TestComponent"/> class that is used 
/// to send an HTTP request. It uses the <see cref="IHttpClientFactory"/> to create an instance of the 
/// <see cref="HttpClient"/> class, which is used to send the HTTP request.
/// </summary>
/// <remarks>
/// Before using this test component, you need to register the necessary services by calling the AddHttpClients()
/// method.
/// </remarks>
internal sealed class HttpClientRequestSender(string name) : TestComponent(name, type: TestStepType.ActionStep)
{
    private readonly OrderedHttpRedirections _visitedUrls = new();

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="context">A <see cref="TestContext"/> object that represents the test context.</param>
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

        _visitedUrls.Clear();

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
            using HttpResponseMessage response = await SendRequestAsync(
                    httpClient, request, settings, context, cancellationToken)
                .ConfigureAwait(false);

            byte[] buffer = await ReadAsByteArrayAsync(response.Content, cancellationToken).ConfigureAwait(false);
            testStep = context.SessionBuilder
                .Build(PropertyBagKeys.HttpStatus, new PropertyBagValue<string>($"{(int)response.StatusCode}"))
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
        if (settings.RetryHttpRequestWhenFailed == true)
        {
            httpClient = httpClientFactory.CreateClient(
                HttpClientConfiguration.HttpClientWithRetryAndNoFollowRedirect);
        }
        else if (settings.RetryHttpRequestWhenFailed == false)
        {
            httpClient = httpClientFactory.CreateClient(
                HttpClientConfiguration.HttpClientWithNoRetryAndNoFollowRedirect);
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

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpClient httpClient,
        HttpRequestMessage request,
        TestSettings settings,
        TestContext context,
        CancellationToken cancellationToken)
    {
        if (settings.FollowHttpRedirectionResponses)
        {
            if (request.RequestUri != null)
            {
                _visitedUrls.Add(request.RequestUri.AbsoluteUri);
            }

            using var instrumentation = new InstrumentationLog(startStopwatch: true);

            var response = await httpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            while (!response.IsSuccessStatusCode && _visitedUrls.Count <= settings.MaxRedirections)
            {
                // It is not recommended to handle redirects by checking if HTTP status code is between 300 and 399,
                // because it does not account for the different types of redirections and their implications. For
                // example, some redirects may change the request method from POST to GET, or require user confirmation
                // before proceeding. Therefore, it is better to use the StatusCode property of the HttpResponseMessage
                // class, which returns a value of the HttpStatusCode enum. This way, we can handle each redirection
                // type appropriately and follow the best practices.

                // Manually check if the response is a redirection
                if (response.StatusCode == HttpStatusCode.Redirect ||
                    response.StatusCode == HttpStatusCode.MovedPermanently ||
                    response.StatusCode == HttpStatusCode.Found ||
                    response.StatusCode == HttpStatusCode.SeeOther ||
                    response.StatusCode == HttpStatusCode.TemporaryRedirect)
                {
                    TestStep testStep = context.SessionBuilder
                        .Build(PropertyBagKeys.HttpStatus, new PropertyBagValue<string>($"{(int)response.StatusCode}"))
                        .Build(PropertyBagKeys.HttpVersion, new PropertyBagValue<string>($"{response.Version}"))
                        .Build(PropertyBagKeys.HttpReasonPhrase, new PropertyBagValue<string?>(response.ReasonPhrase))
                        .Build(PropertyBagKeys.HttpResponseHeaders, GetHeaders(response.Headers))
                        .Build(PropertyBagKeys.HttpResponseTrailingHeaders, GetHeaders(response.TrailingHeaders))
                        .Build(PropertyBagKeys.HttpContentHeaders, GetHeaders(response.Content.Headers))
                        .Build(component: this, instrumentation);
                    context.Progress?.Report(testStep);

                    // Location HTTP header, specifies the absolute or relative URL of the new resource.
                    Uri? redirectUrl = response.Headers.Location;

                    if (redirectUrl != null)
                    {
                        if (!redirectUrl.IsAbsoluteUri)
                        {
                            string lastAbsoluteUri =
                                _visitedUrls.FindLastMatchingItem(url => new Uri(url).IsAbsoluteUri) ??
                                throw new InvalidOperationException("Invalid Redirection Attempt Detected. The server "+
                                "attempted to redirect to an invalid or unrecognized location. Please check the URL "+
                                "or contact the site administrator for assistance.");

                            redirectUrl = new Uri(baseUri: new Uri(lastAbsoluteUri), relativeUri: redirectUrl);
                        }

                        if (_visitedUrls.Add(redirectUrl.ToString()) == false)
                        {
                            // Circular dependency detected
                            throw new InvalidOperationException(
                                $"A circular dependency was detected for the URL {redirectUrl}. " +
                                $"The redirection chain is: {string.Join(" -> ", _visitedUrls)}");
                        }

                        using HttpRequestMessage redirectRequest = CreateHttpRequestMessage(redirectUrl, settings);

                        response = await httpClient
                            .SendAsync(redirectRequest, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
            }

            // Throw an exception if the max number of redirects has been reached
            if (_visitedUrls.Count > settings.MaxRedirections)
            {
                throw new WebException(
                    $"The maximum number of redirects ({settings.MaxRedirections}) has been exceeded for the URL " +
                    $"{request.RequestUri}. The last redirect URL was " +
                    $"{_visitedUrls.FindLastMatchingItem(str => !string.IsNullOrEmpty(str))}.",
                    WebExceptionStatus.ProtocolError);
            }

            return response;
        }
        else
        {
            return await httpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
