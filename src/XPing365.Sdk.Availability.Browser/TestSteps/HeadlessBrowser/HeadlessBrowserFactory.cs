using XPing365.Sdk.Availability.TestSteps.HeadlessBrowser.Internals;

namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

public sealed class HeadlessBrowserFactory : IHeadlessBrowserFactory
{
    public HeadlessBrowserClient CreateClient(BrowserContext context)
    {
        return new PhantomJsBrowserClient(context);
    }
}
