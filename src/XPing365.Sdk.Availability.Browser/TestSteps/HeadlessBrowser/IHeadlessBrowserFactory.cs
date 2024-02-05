namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

public interface IHeadlessBrowserFactory : IDisposable
{
    Task<HeadlessBrowserClient> CreateClientAsync(BrowserContext context);
}
