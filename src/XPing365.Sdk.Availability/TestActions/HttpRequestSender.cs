using XPing365.Sdk.Availability.TestActions.Internals;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Availability.TestActions;

/// <summary>
/// The HttpRequestSender class is a unified interface for sending HTTP requests using either HttpClient or Headless 
/// browser. Users can specify the desired client type in the constructor parameter. If no client type is provided, the 
/// HttpRequestSender class will use HttpClient by default. This component operates without any dependency on other 
/// components.
/// </summary>
/// <remarks>
/// <para>
/// HttpClient is a .NET class that provides a high-level abstraction for sending and receiving HTTP requests and 
/// responses. It is fast, lightweight, and easy to use. However, it does not process HTML responses or run JavaScript 
/// code, which may limit its ability to validate server responses. Headless Browsers are browsers that run without a 
/// graphical user interface, but can still render web pages and execute JavaScript code. They are useful for simulating 
/// user interactions and testing dynamic web applications. However, they are slower, heavier, and more complex than 
/// HttpClient. 
/// </para>
/// <para>
/// Depending on your testing needs, you can choose either or both of these mechanisms to create and run your HTTP 
/// requests with XPing365 SDK.
/// </para>
/// <note type="tip">
/// The HttpRequestSender class does not support using both HttpClient and Headless browser in the same testing 
/// pipeline. If you try to do so, the test session results from one client will be overwritten by the results from the 
/// other client, and you will lose some data. If you need to test the same URL with both clients, you should create two 
/// separate testing pipelines, one for each client type, and run them independently.
/// </note>
/// </remarks>
public class HttpRequestSender(Client client = Client.HttpClient) : 
    TestComponent(name: StepName, type: TestStepType.ActionStep)
{
    private readonly Lazy<HttpClientRequestSender> _httpClientRequestSender = new(
        valueFactory: () => new HttpClientRequestSender($"{StepName} ({nameof(Client.HttpClient)})"), 
        isThreadSafe: true);
    private readonly Lazy<HeadlessBrowserRequestSender> _headlessBrowserRequestSender = new(
        valueFactory: () => new HeadlessBrowserRequestSender($"{StepName} ({nameof(Client.HeadlessBrowser)})"), 
        isThreadSafe: true);
    
    private readonly Client _client = client;

    public const string StepName = "Http request sender";

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
    public override Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        return _client switch
        {
            Client.HttpClient => _httpClientRequestSender.Value.HandleAsync(
                url, settings, context, serviceProvider, cancellationToken),
            Client.HeadlessBrowser => _headlessBrowserRequestSender.Value.HandleAsync(
                url, settings, context, serviceProvider, cancellationToken),
            _ => throw new NotSupportedException(Errors.IncorrectClientType)
        };
    }
}
