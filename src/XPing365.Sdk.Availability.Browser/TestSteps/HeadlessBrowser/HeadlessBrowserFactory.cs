using Microsoft.Playwright;

namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

public sealed class HeadlessBrowserFactory : IHeadlessBrowserFactory
{
    private bool _disposedValue;
    private IPlaywright? _playwright;

    public async Task<HeadlessBrowserClient> CreateClientAsync(BrowserContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        _playwright ??= await Playwright.CreateAsync().ConfigureAwait(false);

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = true
        };

        return context.Type switch
        {
            BrowserType.Webkit => 
                new HeadlessBrowserClient(
                    browser: await _playwright.Webkit.LaunchAsync(launchOptions).ConfigureAwait(false), context),

            BrowserType.Firefox => 
                new HeadlessBrowserClient(
                    browser: await _playwright.Firefox.LaunchAsync(launchOptions).ConfigureAwait(false), context),

            _ => new HeadlessBrowserClient(
                browser: await _playwright.Chromium.LaunchAsync(launchOptions).ConfigureAwait(false), context),
        };
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _playwright?.Dispose();
            }

            _playwright = null;
            _disposedValue = true;
        }
    }
}
