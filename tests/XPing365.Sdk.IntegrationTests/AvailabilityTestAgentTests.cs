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
            step.Name.StartsWith(HttpRequestSender.StepName, StringComparison.InvariantCulture) &&
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

    [Test]
    public async Task HttpRequestSenderComponentFollowsRedirectWhenConfigured()
    {
        // Arrange
        string destinationServerAddress = "http://localhost:8081/";
        HttpStatusCode redirectionStatus = HttpStatusCode.MovedPermanently;

        using Task destinationServer = InMemoryHttpServer.TestServer(
            responseBuilder: (response) =>
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close(); // By closing a response it can be send to client.
            },
            requestReceived: (request) => { },
            uriPrefixes: [new Uri(destinationServerAddress)]);

        var settings = TestSettings.DefaultForHttpClient;
        settings.FollowHttpRedirectionResponses = true;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            responseBuilder: (response) =>
            {
                response.StatusCode = (int)redirectionStatus;
                response.Headers.Add(HeaderNames.Location, destinationServerAddress);
                response.Close(); // By closing a response it can be send to client.
            }, settings: settings).ConfigureAwait(false);

        await destinationServer.WaitAsync(cancellationToken: default).ConfigureAwait(false);

        // Assert
        var httpRequestSteps = session.Steps.Where(s => 
            s.Name.StartsWith(HttpRequestSender.StepName, StringComparison.InvariantCulture)).ToList();
        var redirectionStep = httpRequestSteps.First();
        var destinationStep = httpRequestSteps.Last();

        var redirectionStatusCode = redirectionStep.PropertyBag!.GetProperty<PropertyBagValue<string>>(
            PropertyBagKeys.HttpStatus).Value;
        var redirectionUrl = redirectionStep.PropertyBag!.GetProperty<PropertyBagValue<Dictionary<string, string>>>(
            PropertyBagKeys.HttpResponseHeaders).Value[HeaderNames.Location.ToUpperInvariant()];
        var destinationStatusCode = destinationStep.PropertyBag!.GetProperty<PropertyBagValue<string>>(
            PropertyBagKeys.HttpStatus).Value;

        Assert.Multiple(() =>
        {
            Assert.That(httpRequestSteps, Has.Count.EqualTo(2));
            Assert.That(redirectionStep, Is.Not.Null);
            Assert.That(redirectionStatusCode, Is.EqualTo($"{(int)redirectionStatus}"));
            Assert.That(redirectionUrl, Is.EqualTo(destinationServerAddress));
            Assert.That(destinationStep, Is.Not.Null);
            Assert.That(destinationStatusCode, Is.EqualTo($"{(int)HttpStatusCode.OK}"));
        });
    }

    [Test]
    public async Task HttpRequestSenderComponentDetectsCircularDependency()
    {
        // Arrange
        // The redirect URL is on the same server as the original URL
        string destinationServerAddress = InMemoryHttpServer.GetTestServerAddress().AbsoluteUri;

        var settings = TestSettings.DefaultForHttpClient;
        settings.FollowHttpRedirectionResponses = true;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            responseBuilder: (response) =>
            {
                response.StatusCode = (int)HttpStatusCode.MovedPermanently;
                response.Headers.Add(HeaderNames.Location, destinationServerAddress);
                response.Close(); // By closing a response it can be send to client.
            }, settings: settings).ConfigureAwait(false);

        // Assert
        var httpRequestSteps = session.Steps.Where(s =>
            s.Name.StartsWith(HttpRequestSender.StepName, StringComparison.InvariantCulture)).ToList();
        var redirectionStep = httpRequestSteps.First();
        var destinationStep = httpRequestSteps.Last();

        var redirectionStatusCode = redirectionStep.PropertyBag!.GetProperty<PropertyBagValue<string>>(
            PropertyBagKeys.HttpStatus).Value;
        var redirectionUrl = redirectionStep.PropertyBag!.GetProperty<PropertyBagValue<Dictionary<string, string>>>(
            PropertyBagKeys.HttpResponseHeaders).Value[HeaderNames.Location.ToUpperInvariant()];

        Assert.Multiple(() =>
        {
            Assert.That(httpRequestSteps, Has.Count.EqualTo(2));
            Assert.That(redirectionStep, Is.Not.Null);
            Assert.That(redirectionStep.Result, Is.EqualTo(TestStepResult.Succeeded));
            Assert.That(destinationStep.Result, Is.EqualTo(TestStepResult.Failed));
            Assert.That(destinationStep.ErrorMessage, Does.Contain("A circular dependency was detected for the " +
                "URL http://localhost:8080/. The redirection chain is: http://localhost:8080/"));
        });
    }

    [Test]
    public async Task HttpRequestSenderComponentReportsProgressForAllItsStepsWhenRedirectionHappens()
    {
        // Arrange
        var mockProgress = _serviceProvider.GetService<IProgress<TestStep>>();

        // The redirect URL is on the same server as the original URL
        string destinationServerAddress = InMemoryHttpServer.GetTestServerAddress().AbsoluteUri;
        var settings = TestSettings.DefaultForHttpClient;
        settings.FollowHttpRedirectionResponses = true;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            responseBuilder: (response) =>
            {
                response.StatusCode = (int)HttpStatusCode.MovedPermanently;
                response.Headers.Add(HeaderNames.Location, destinationServerAddress);
                response.Close(); // By closing a response it can be send to client.
            }, settings: settings).ConfigureAwait(false);

        // Assert
        Mock.Get(mockProgress!).Verify(p => p.Report(It.IsAny<TestStep>()), Times.Exactly(session.Steps.Count));
    }

    [Test]
    public async Task HttpRequestSenderComponentRestrictsNumberOfRedirectionsWhenConfigured()
    {
        // Arrange two HTTP redirections localhost:8080 -> localhost:8081 -> localhost:8082
        string destinationServerAddress = "http://localhost:8081/";
        HttpStatusCode redirectionStatus = HttpStatusCode.MovedPermanently;

        using Task destinationServer = InMemoryHttpServer.TestServer(
            responseBuilder: (response) =>
            {
                response.StatusCode = (int)HttpStatusCode.MovedPermanently;
                response.Headers.Add(HeaderNames.Location, "http://localhost:8082/");
                response.Close(); // By closing a response it can be send to client.
            },
            requestReceived: (request) => { },
            uriPrefixes: [new Uri(destinationServerAddress)]);

        var settings = TestSettings.DefaultForHttpClient;
        settings.FollowHttpRedirectionResponses = true;
        settings.MaxRedirections = 1;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            responseBuilder: (response) =>
            {
                response.StatusCode = (int)redirectionStatus;
                response.Headers.Add(HeaderNames.Location, destinationServerAddress);
                response.Close(); // By closing a response it can be send to client.
            }, settings: settings).ConfigureAwait(false);

        await destinationServer.WaitAsync(cancellationToken: default).ConfigureAwait(false);

        // Assert
        var httpRequestSteps = session.Steps.Where(s =>
            s.Name.StartsWith(HttpRequestSender.StepName, StringComparison.InvariantCulture)).ToList();
        var destinationStep = httpRequestSteps.Last();

        Assert.Multiple(() =>
        {
            Assert.That(destinationStep.Result, Is.EqualTo(TestStepResult.Failed));
            Assert.That(destinationStep.ErrorMessage, Does.Contain($"The maximum number of redirects " +
                $"({settings.MaxRedirections}) has been exceeded for the URL http://localhost:8080/. The last " +
                $"redirect URL was http://localhost:8081/"));
        });
    }

    [Test]
    public async Task HttpRequestSenderComponentFollowsRelativeRedirectedUrl()
    {
        bool redirected = false;

        // Arrange
        using Task redirectionServer = InMemoryHttpServer.TestServer(
            responseBuilder: (response) =>
            {
                if (!redirected)
                {
                    response.StatusCode = (int)HttpStatusCode.Moved;
                    response.Headers.Add(HeaderNames.Location, "/home");
                    response.Close(); // By closing a response it can be send to client.
                    redirected = true;
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close(); // By closing a response it can be send to client.
                }
            },
            requestReceived: (request) => { },
            uriPrefixes: [new Uri("http://localhost:8081/"), new Uri("http://localhost:8081/home/")]);

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            responseBuilder: (response) =>
            {
                response.StatusCode = (int)HttpStatusCode.Moved;
                response.Headers.Add(HeaderNames.Location, "http://localhost:8081/");
                response.Close(); // By closing a response it can be send to client.
            }).ConfigureAwait(false);

        await redirectionServer.WaitAsync(cancellationToken: default).ConfigureAwait(false);

        // Assert
        var httpRequestSteps = session.Steps.Where(s =>
            s.Name.StartsWith(HttpRequestSender.StepName, StringComparison.InvariantCulture)).ToList();
        var destinationStep = httpRequestSteps.Last();

        var destinationStatusCode = destinationStep.PropertyBag!.GetProperty<PropertyBagValue<string>>(
            PropertyBagKeys.HttpStatus).Value;
        var destinationUrl = destinationStep.PropertyBag!.GetProperty<NonSerializable<HttpResponseMessage>>(
            PropertyBagKeys.HttpResponseMessage).Value;

        Assert.Multiple(() =>
        {
            Assert.That(httpRequestSteps, Has.Count.EqualTo(3));
            Assert.That(destinationStep, Is.Not.Null);
            Assert.That(destinationStatusCode, Is.EqualTo($"{(int)HttpStatusCode.OK}"));
            Assert.That(destinationUrl, Is.Not.Null);
            Assert.That(destinationUrl.RequestMessage!.RequestUri!.AbsoluteUri, 
                Is.EqualTo($"http://localhost:8081/home"));
        });
    }

    [Test]
    public async Task HttpRequestSenderComponentDoesNotFollowHttpRedirectWhenFollowRedirectionIsDisabled()
    {
        // Arrange
        string destinationServerAddress = "http://localhost:8080/destination";
        var settings = TestSettings.DefaultForHttpClient;
        settings.FollowHttpRedirectionResponses = false;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            responseBuilder: (response) =>
            {
                response.StatusCode = (int)HttpStatusCode.MovedPermanently;
                response.Headers.Add(HeaderNames.Location, destinationServerAddress);
                response.Close(); // By closing a response it can be send to client.
            }, settings: settings).ConfigureAwait(false);

        // Assert
        var httpRequestSteps = session.Steps.Where(s =>
            s.Name.StartsWith(HttpRequestSender.StepName, StringComparison.InvariantCulture)).ToList();

        Assert.That(httpRequestSteps, Has.Count.EqualTo(1));
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
