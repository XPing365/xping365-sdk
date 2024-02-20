using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Availability.TestActions;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.IntegrationTests.HttpServer;
using XPing365.Sdk.IntegrationTests.TestFixtures;

namespace XPing365.Sdk.IntegrationTests;

[SingleThreaded]
[SetUpFixture]
[TestFixtureSource(typeof(TestFixtureProvider), nameof(TestFixtureProvider.ServiceProvider))]
public class BrowserTestAgentTests(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [SetUp]
    public void Setup()
    {
        var progressMock = Mock.Get(_serviceProvider.GetRequiredService<IProgress<TestStep>>());
        progressMock?.Reset();
    }

    [Test]
    public async Task TestSessionFromBrowserTestAgentIsMarkedCompletedWhenSucceeded()
    {
        // Arrange
        const TestSessionState expectedTestSessionState = TestSessionState.Completed;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
                responseBuilder: SimplePageResponseBuilder)
            .ConfigureAwait(false);

        // Assert
        Assert.That(session.State, Is.EqualTo(expectedTestSessionState));
    }

    [Test]
    public async Task ResponseHttpContentTypeHeaderHasCorrectValue()
    {
        // Arrange
        const string expectedContentType = "text/html";
        const string expectedCharSet = "utf-8";

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
                responseBuilder: SimplePageResponseBuilder)
            .ConfigureAwait(false);

        var contentType = session.GetNonSerializablePropertyBagValue<HttpResponseMessage>(
            PropertyBagKeys.HttpResponseMessage)!.Content.Headers.ContentType;

        // Assert
        Assert.That(contentType, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(contentType.MediaType, Is.EqualTo(expectedContentType));
            Assert.That(contentType.CharSet, Is.EqualTo(expectedCharSet));
        });
    }

    [Test]
    public async Task TestSessionFromBrowserTestAgentContainsAllTestStepsWhenSucceeded()
    {
        // Arrange
        const int expectedTestStepsCount = 3;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
                responseBuilder: SimplePageResponseBuilder)
            .ConfigureAwait(false);

        // Assert
        Assert.That(session.Steps, Has.Count.EqualTo(expectedTestStepsCount));
    }

    [Test]
    public async Task DnsLookupTestStepHasResolvedIPAddressesWhenSucceeded()
    {
        // Arrange 
        PropertyBagKey expectedBag = PropertyBagKeys.DnsResolvedIPAddresses;

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
                responseBuilder: SimplePageResponseBuilder)
            .ConfigureAwait(false);

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
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
                responseBuilder: SimplePageResponseBuilder)
            .ConfigureAwait(false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(session.Steps.Any(step => step.Name == IPAddressAccessibilityCheck.StepName), Is.True);
            Assert.That(session.TryGetPropertyBagValue(expectedBag, out PropertyBagValue<string>? ipaddress), Is.True);
        });
    }

    [Test]
    public async Task HeadlessBrowserRequestSenderSendsUserAgentHttpHeaderWhenConfigured()
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
                responseBuilder: SimplePageResponseBuilder,
                requestReceived: ValidateRequest, 
                settings: settings)
            .ConfigureAwait(false);
    }

    [Test]
    public async Task HeadlessBrowserRequestSenderHasExpectedErrorMessageWhenTimeouts()
    {
        // Arrange
        const string expectedErrMsg =
            "Error 1000: Message: Timeout 1000ms exceeded.\nCall log:\n  - navigating to \"http://localhost:8080/\", " +
            "waiting until \"load\"";

        TestSettings settings = TestSettings.DefaultForHttpClient;
        settings.PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpRequestTimeout, TimeSpan.FromSeconds(1));

        void ResponseBuilder(HttpListenerResponse response)
        {
            TimeSpan timeout = settings.PropertyBag.GetProperty<TimeSpan>(PropertyBagKeys.HttpRequestTimeout);

            // Delay response time which is > than TimeOut value defined in TestSettings
            // so we can test if the BrowserTestAgent correctly reacts to timeout configuration.
            Thread.Sleep(timeout + TimeSpan.FromSeconds(1));
            var file = new FileInfo(@"HttpServer/Pages/SimplePage.html");
            using var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);

            // Set the headers for the response
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = fileStream.Length;

            // Write the file contents to the response stream
            fileStream.CopyTo(response.OutputStream);
            fileStream.Close();
            response.Close();
        }

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
            ResponseBuilder, settings: settings).ConfigureAwait(false);

        var failedStep = session.Steps.FirstOrDefault(step =>
            step.Name == HeadlessBrowserRequestSender.StepName &&
            step.Result == TestStepResult.Failed);

        // Assert
        Assert.That(failedStep, Is.Not.Null);
        Assert.That(failedStep.ErrorMessage, Is.EqualTo(expectedErrMsg));
    }

    [Test]
    public async Task HeadlessBrowserRequestSenderStepAddsHttpResponseContentToPropertyBag()
    {
        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
                responseBuilder: SimplePageResponseBuilder)
            .ConfigureAwait(false);

        // Assert
        PropertyBagValue<byte[]>? byteArray = null;

        Assert.Multiple(() =>
        {
            Assert.That(session.TryGetPropertyBagValue(PropertyBagKeys.HttpContent, out byteArray), Is.True);
            Assert.That(byteArray is not null);
        });
    }

    [Test]
    public async Task HeadlessBrowserExecutesJavascriptWhenIncludedInWebPage()
    {
        // Arrange
        string expectedText = $"{DateTime.Now.Year}";

        // Act
        TestSession session = await GetTestSessionFromInMemoryHttpTestServer(
                responseBuilder: JavascriptPageResponseBuilder)
            .ConfigureAwait(false);

        var byteArray = session.GetPropertyBagValue<byte[]>(PropertyBagKeys.HttpContent);
        var content = Encoding.UTF8.GetString(byteArray!);

        // Assert
        Assert.That(content.Contains(expectedText, StringComparison.InvariantCulture), Is.True);
    }

    private async Task<TestSession> GetTestSessionFromInMemoryHttpTestServer(
        Action<HttpListenerResponse> responseBuilder,
        Action<HttpListenerRequest>? requestReceived = null,
        TestComponent? component = null,
        TestSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        static void RequestReceived(HttpListenerRequest request) { };

        using Task testServer = InMemoryHttpServer.TestServer(
            responseBuilder, 
            requestReceived ?? RequestReceived, 
            cancellationToken);

        var testAgent = _serviceProvider.GetRequiredKeyedService<TestAgent>(serviceKey: "BrowserClient");

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
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await testServer.WaitAsync(cancellationToken).ConfigureAwait(false);

        return session;
    }

    private static void SimplePageResponseBuilder(HttpListenerResponse response) =>
        PageResponseBuilder(response, new FileInfo(@"HttpServer/Pages/SimplePage.html"));

    private static void JavascriptPageResponseBuilder(HttpListenerResponse response) =>
        PageResponseBuilder(response, new FileInfo(@"HttpServer/Pages/JavascriptPage.html"));

    private static void PageResponseBuilder(HttpListenerResponse response, FileInfo fileToRespond)
    {
        using var fileStream = new FileStream(fileToRespond.FullName, FileMode.Open, FileAccess.Read);

        // Set the headers for the response
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = fileStream.Length;

        // Write the file contents to the response stream
        fileStream.CopyTo(response.OutputStream);
        fileStream.Close();
        response.Close();
    }
}
