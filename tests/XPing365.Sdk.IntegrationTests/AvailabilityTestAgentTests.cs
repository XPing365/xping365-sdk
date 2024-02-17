using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using XPing365.Sdk.Availability.TestActions;
using XPing365.Sdk.Availability.TestValidators;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.IntegrationTests.HttpServer;
using XPing365.Sdk.IntegrationTests.TestFixtures;

namespace XPing365.Sdk.IntegrationTests;

[SetUpFixture]
[TestFixtureSource(typeof(TestFixtureProvider), nameof(TestFixtureProvider.ServiceProvider))]
public class AvailabilityTestAgentTests(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [SetUp]
    public void Setup()
    {
        var progressMock = Mock.Get(_serviceProvider.GetRequiredService<IProgress<TestStep>>());
        progressMock?.Reset();
    }

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
        Assert.Multiple(() =>
        {
            Assert.That(session.Steps.Any(step => step.Name == DnsLookup.StepName), Is.True);
            Assert.That(session.TryGetPropertyBagValue(expectedBag, out PropertyBagValue<string[]>? ips), Is.True);
        });
    }

    [Test]
    public async Task IPAddressAccessibilityCheckStepHasValidatedIPAddressWhenSucceeded()
    {
        // Arrange 
        PropertyBagKey expectedBag = PropertyBagKeys.IPAddress;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer().ConfigureAwait(false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(session.Steps.Any(step => step.Name == IPAddressAccessibilityCheck.StepName), Is.True);
            Assert.That(session.TryGetPropertyBagValue(expectedBag, out PropertyBagValue<string>? ipAddress), Is.True);
        });
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

        TestSettings testSettings = TestSettings.DefaultForHttpClient;
        testSettings.RetryHttpRequestWhenFailed = false;
        testSettings.FollowHttpRedirectionResponses = false;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            ResponseBuilder, settings: testSettings).ConfigureAwait(false);

        // Assert
        PropertyBagValue<string>? code = null;

        Assert.Multiple(() =>
        {
            Assert.That(session.TryGetPropertyBagValue(PropertyBagKeys.HttpStatus, out code), Is.True);
            Assert.That(Enum.Parse<HttpStatusCode>(code!.Value), Is.EqualTo(httpStatusCode));
        });
    }

    [Test]
    public async Task SentHttpRequestStepHasExpectedErrorMessageWhenTimeouts()
    {
        // Arrange
        const string expectedErrMsg = 
            "Error 1000: Message: The request was canceled due to the configured HttpClient.Timeout of 1 seconds " +
            "elapsing.";
        TestSettings settings = TestSettings.DefaultForHttpClient;
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
            step.Name == HttpClientRequestSender.StepName &&
            step.Result == TestStepResult.Failed &&
            step.ErrorMessage == expectedErrMsg), Is.True);
    }

    [Test]
    public async Task SendHttpRequestStepSendsUserAgentHttpHeaderWhenConfigured()
    {
        // Arrange
        const string userAgent = "Chrome/51.0.2704.64 Safari/537.36";

        TestSettings settings = TestSettings.DefaultForHttpClient;
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
        PropertyBagValue<byte[]>? byteArray = null;

        Assert.Multiple(() =>
        {
            Assert.That(session.TryGetPropertyBagValue(PropertyBagKeys.HttpContent, out byteArray), Is.True);
            Assert.That(byteArray is not null && Encoding.UTF8.GetString(byteArray.Value) == responseContent);
        });
    }

    [Test]
    public async Task ServerContentResponseValidatorDoesNotThrowWhenResponseDoesNotContainContent()
    {
        // Arrange
        const bool expectedValidityResult = true;
        var serverContentValidator = new HttpResponseContentValidator(
            isValid: (byte[] buffer, HttpContentHeaders contentHeaders) => expectedValidityResult);

        // Act
        TestSession session = 
            await GetTestSessionFromInMemoryHttpTestServer(component: serverContentValidator).ConfigureAwait(false);

        // Assert
        Assert.That(session.IsValid, Is.EqualTo(expectedValidityResult));
    }

    [Test]
    public async Task ProgressInstanceIsCalledForEveryStepWhenProgressIsProvided()
    {
        // Arrange
        var mockProgress = _serviceProvider.GetService<IProgress<TestStep>>();
        var validationPipeline = new Pipeline(components: [
            new HttpResponseContentValidator(isValid: (byte[] buffer, HttpContentHeaders contentHeaders) => true),
            new HttpStatusCodeValidator(isValid: code => true),
            new HttpResponseHeadersValidator(isValid: headers => true)
        ]);

        // Act
        TestSession session =
            await GetTestSessionFromInMemoryHttpTestServer(component: validationPipeline).ConfigureAwait(false);

        // Assert
        Mock.Get(mockProgress!).Verify(p => p.Report(It.IsAny<TestStep>()), Times.Exactly(session.Steps.Count));
    }

    private async Task<TestSession> GetTestSessionFromInMemoryHttpTestServer(
        Action<HttpListenerResponse>? responseBuilder = null,
        Action<HttpListenerRequest>? requestReceived = null,
        TestComponent? component = null,
        TestSettings? settings = null)
    {
        using var cts = new CancellationTokenSource();

        static void ResponseBuilder(HttpListenerResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close(); // By closing a response it can be send to client.
        }
        static void RequestReceived(HttpListenerRequest request) { };

        using Task testServer = InMemoryHttpServer.TestServer(
            responseBuilder ?? ResponseBuilder,
            requestReceived ?? RequestReceived, 
            cts.Token);
        
        var testAgent = _serviceProvider.GetRequiredKeyedService<TestAgent>(serviceKey: "HttpClient");

        if (component != null)
        {
            if (testAgent.Container == null)
            {
                testAgent.Container = new Pipeline(components: [component]);
            }
            else
            {
                testAgent.Container.AddComponent(component);
            }
        }

        TestSession session = await testAgent
            .RunAsync(
                url: InMemoryHttpServer.GetTestServerAddress(),
                settings: settings ?? TestSettings.DefaultForHttpClient, 
                cancellationToken: cts.Token)
            .ConfigureAwait(false);

        await testServer.WaitAsync(cts.Token).ConfigureAwait(false);

        return session;
    }
}
