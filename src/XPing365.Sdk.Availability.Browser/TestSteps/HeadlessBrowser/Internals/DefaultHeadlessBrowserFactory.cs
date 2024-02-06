using Microsoft.Playwright;

namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser.Internals;

// This class is being used in DependencyInjection and as such shows as never instantiated thus disabling the warning.

#pragma warning disable CA1812 // internal class that is apparently never instantiated
internal sealed class DefaultHeadlessBrowserFactory : IHeadlessBrowserFactory
#pragma warning restore CA1812 // internal class that is apparently never instantiated
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
