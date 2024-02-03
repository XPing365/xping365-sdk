using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Availability;


// PhantomJS is a headless WebKit with JavaScript API 

public sealed class BrowserTestAgent(IServiceProvider serviceProvider, IHeadlessBrowserFactory headlessBrowserFactory) : 
    TestAgent(serviceProvider, new Pipeline(name: PipelineName, [
        new DnsLookup(), 
        new IPAddressAccessibilityCheck(), 
        new HeadlessBrowserRequestSender(headlessBrowserFactory)]))
{
    public const string PipelineName = "Headless browser pipeline";
}
