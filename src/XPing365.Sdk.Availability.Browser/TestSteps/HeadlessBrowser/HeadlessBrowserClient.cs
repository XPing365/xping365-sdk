using System.Diagnostics;
using Microsoft.Playwright;
using XPing365.Sdk.Availability.Browser.TestSteps.HeadlessBrowser.Internals;

namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class HeadlessBrowserClient(IBrowser browser, BrowserContext context) : IDisposable, IAsyncDisposable
{
    private IBrowser _browser = browser;

    public BrowserContext Context { get; init; } = context;

    public string Name => _browser.BrowserType.Name;

    public string Version => _browser.Version;

    public async Task<WebPage> GetAsync(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));

        var options = new BrowserNewPageOptions
        {
            UserAgent = Context.UserAgent
        };

        var page = await _browser.NewPageAsync(options).ConfigureAwait(false);

        // Navigate to the website and wait for the response
        var response = await page.GotoAsync(
            url: url.AbsoluteUri, 
            options: new PageGotoOptions { Timeout = (float)Context.Timeout.TotalMilliseconds })
            .ConfigureAwait(false);
        var webpage = await new WebPageBuilder()
            .Build(page)
            .Build(response)
            .GetWebPageAsync()
            .ConfigureAwait(false);

        return webpage;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_browser is IDisposable disposable)
            {
                disposable.Dispose();
                _browser = null!;
            }
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync().ConfigureAwait(false);
        }

        _browser = null!;
    }

    private string GetDebuggerDisplay() => Name;
}
