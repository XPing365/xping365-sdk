using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using XPing365.Sdk.Availability;
using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Core;
using XPing365.Sdk.IntegrationTests.HttpServer;
using XPing365.Sdk.IntegrationTests.TestFixtures;

namespace XPing365.Sdk.IntegrationTests;

[SetUpFixture]
[SingleThreaded]
[TestFixtureSource(typeof(TestFixtureProvider), nameof(TestFixtureProvider.ServiceProvider))]
public class AvailabilityTestAgentTests(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [Test]
    public async Task TestSessionFromAvailabilityTestAgentIsMarkedCompletedWhenSucceeded()
    {
        // Arrange
        const TestSessionState expectedTestSessionState = TestSessionState.Completed;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer().ConfigureAwait(false);

        // Assert
        Assert.That(session.State, Is.EqualTo(expectedTestSessionState));
    }

    [Test]
    public async Task TestSessionFromAvailabilityTestAgentContainsAllTestStepsWhenSucceeded()
    {
        // Arrange
        const int expectedTestStepsCount = 3;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer().ConfigureAwait(false);

        // Assert
        Assert.That(session.Steps, Has.Count.EqualTo(expectedTestStepsCount));
    }

    [Test]
    public async Task DnsLookupTestStepHasResolvedIPAddressesWhenSucceeded()
    {
        // Arrange 
        PropertyBagKey expectedBag = PropertyBagKeys.DnsResolvedIPAddresses;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer().ConfigureAwait(false);

        // Assert
        Assert.That(
            session.Steps.Any(step =>
                step.Name == DnsLookup.StepName &&
                step.PropertyBag.TryGetProperty(expectedBag, out var ipaddresses)), Is.True);
    }

    [Test]
    public async Task IPAddressAccessibilityCheckStepHasValidatedIPAddressWhenSucceeded()
    {
        // Arrange 
        PropertyBagKey expectedBag = PropertyBagKeys.IPAddress;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer().ConfigureAwait(false);

        // Assert
        Assert.That(
            session.Steps.Any(step =>
                step.Name == IPAddressAccessibilityCheck.StepName && 
                step.PropertyBag.TryGetProperty(expectedBag, out var ipaddress)), Is.True);
    }

    [Test]
    [TestCase(HttpStatusCode.Accepted)]
    [TestCase(HttpStatusCode.OK)]
    [TestCase(HttpStatusCode.Moved)]
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.Redirect)]
    public async Task SentHttpRequestStepHasCorrectHttpStatusCodeWhenSucceeded(HttpStatusCode httpStatusCode)
    {
        // Arrange
        void ResponseBuilder(HttpListenerResponse response)
        {
            response.StatusCode = (int)httpStatusCode;
            response.Close(); // By closing a response it can be send to client.
        }

        TestSettings testSettings = TestSettings.DefaultForAvailability;
        testSettings.RetryHttpRequestWhenFailed = false;
        testSettings.FollowHttpRedirectionResponses = false;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            ResponseBuilder, settings: testSettings).ConfigureAwait(false);

        // Assert
        HttpStatusCode? code = null;

        Assert.Multiple(() =>
        {
            Assert.That(
                session.Steps.Any(step => 
                    step.PropertyBag.TryGetProperty(PropertyBagKeys.HttpStatus, out code)), Is.True);
            Assert.That(code, Is.EqualTo(httpStatusCode));
        });
    }

    [Test]
    public async Task SentHttpRequestStepHasExpectedErrorMessageWhenTimeouts()
    {
        // Arrange
        const string expectedErrMsg = "Error 1000: Exception of type System.Threading.Tasks.TaskCanceledException " +
            "occured. Message: The request was canceled due to the configured HttpClient.Timeout of 1 seconds " +
            "elapsing..";
        TestSettings settings = TestSettings.DefaultForAvailability;
        settings.PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpRequestTimeout, TimeSpan.FromSeconds(1));

        void ResponseBuilder(HttpListenerResponse response)
        {
            TimeSpan timeout = settings.PropertyBag.GetProperty<TimeSpan>(PropertyBagKeys.HttpRequestTimeout);

            // Delay response time which is > than TimeOut value defined in TestSettings
            // so we can test if the agent correctly reacts to timeout configuration.
            Thread.Sleep(timeout + TimeSpan.FromSeconds(1));
        }

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            ResponseBuilder, settings: settings).ConfigureAwait(false);

        // Assert
        Assert.That(session.Steps.Any(step =>
            step.Name == SendHttpRequest.StepName &&
            step.Result == TestStepResult.Failed &&
            step.ErrorMessage == expectedErrMsg), Is.True);
    }

    [Test]
    public async Task SendHttpRequestStepSendsUserAgentHttpHeaderWhenConfigured()
    {
        // Arrange
        const string userAgent = "Chrome/51.0.2704.64 Safari/537.36";

        TestSettings settings = TestSettings.DefaultForAvailability;
        var httpRequestHeaders = new Dictionary<string, IEnumerable<string>>
        {
            { HeaderNames.UserAgent, [userAgent] }
        };
        settings.PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpRequestHeaders, httpRequestHeaders);
        
        static void ValidateRequest(HttpListenerRequest request)
        {
            // Assert
            Assert.That(request.UserAgent, Is.Not.Null);
            Assert.That(request.UserAgent, Is.EqualTo(userAgent));
        }

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            requestReceived: ValidateRequest, settings: settings).ConfigureAwait(false);
    }

    [Test]
    public async Task SendHttpRequestStepAddsHttpResponseContentToPropertyBag()
    {
        // Arrange
        const string responseContent = "Test";
        static void ResponseBuilder(HttpListenerResponse response)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(responseContent);
            response.ContentLength64 = byteArray.Length;
            response.OutputStream.Write(
                buffer: byteArray,
                offset: 0, 
                count: responseContent.Length);
            
            response.Close(); // By closing a response it can be send to client.
        }

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(ResponseBuilder).ConfigureAwait(false);

        // Assert
        byte[]? byteArray = null;

        Assert.Multiple(() =>
        {
            Assert.That(
                session.Steps.Any(step =>
                    step.PropertyBag.TryGetProperty(PropertyBagKeys.HttpContent, out byteArray)), Is.True);
            Assert.That(byteArray is not null && Encoding.UTF8.GetString(byteArray) == responseContent);
        });
    }

    private async Task<TestSession> GetTestSessionFromInMemoryHttpTestServer(
        Action<HttpListenerResponse>? responseBuilder = null,
        Action<HttpListenerRequest>? requestReceived = null,
        TestSettings? settings = null)
    {
        using var tokenSource = new CancellationTokenSource();
        static void ResponseBuilder(HttpListenerResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close(); // By closing a response it can be send to client.
        }
        static void RequestReceived(HttpListenerRequest request) { };

        _ = InMemoryHttpServer.TestServer(
            responseBuilder ?? ResponseBuilder,
            requestReceived ?? RequestReceived,
            tokenSource.Token);
        
        var testAgent = _serviceProvider.GetRequiredService<AvailabilityTestAgent>();

        TestSession session = await testAgent
            .RunAsync(
                InMemoryHttpServer.GetTestServerAddress(),
                settings ?? TestSettings.DefaultForAvailability)
            .ConfigureAwait(false);
            
        // Notify InMemoryHttpServer to dispose listener.
        await tokenSource.CancelAsync().ConfigureAwait(false);

        return session;
    }
}
