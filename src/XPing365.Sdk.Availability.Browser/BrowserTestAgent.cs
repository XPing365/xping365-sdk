using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Availability;

/// <summary>
/// The BrowserTestAgent class is a concrete implementation of the <see cref="TestAgent"/> class that is used to 
/// perform availability tests using headless browsers, such as Chromium, Firefox, and WebKit provided by the Playwright 
/// library. This class consist of the following action test steps: 
/// <see cref="DnsLookup"/>, <see cref="IPAddressAccessibilityCheck"/> and <see cref="HeadlessBrowserRequestSender"/> to 
/// perform the availability and monitoring tests. All action steps are performed in a specific order, and their results 
/// are added as <see cref="TestStep"/> results to the <see cref="TestSession"/> object. Any failures can be retrieved 
/// from the <see cref="TestSession.Failures"/> property along with the error description.
/// </summary>
/// <example>
/// <code>
/// using XPing365.Sdk.Availability.Browser.DependencyInjection;
/// 
/// Host.CreateDefaultBuilder()
///     .ConfigureServices(services =>
///     {
///         services.AddBrowserTestAgent();
///     });
/// 
/// var testAgent = _serviceProvider.GetRequiredService&lt;BrowserTestAgent&gt;();
/// 
/// TestContext session = await testAgent
///     .RunAsync(
///         new Uri("www.demoblaze.com"),
///         TestSettings.DefaultForBrowser)
///     .ConfigureAwait(false);
/// </code>
/// </example>
/// <param name="headlessBrowserFactory"><see cref="IHeadlessBrowserFactory"/> implementation instance.</param>
/// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
public sealed class BrowserTestAgent(IServiceProvider serviceProvider, IHeadlessBrowserFactory headlessBrowserFactory) : 
    TestAgent(serviceProvider, new Pipeline(name: PipelineName, [
        new DnsLookup(), 
        new IPAddressAccessibilityCheck(), 
        new HeadlessBrowserRequestSender(headlessBrowserFactory)]))
{
    public const string PipelineName = "Headless browser pipeline";
}
