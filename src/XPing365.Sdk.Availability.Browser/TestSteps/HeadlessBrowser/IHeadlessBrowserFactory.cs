namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

public interface IHeadlessBrowserFactory
{
    HeadlessBrowserClient CreateClient(BrowserContext context);
}
