using System.Diagnostics;
using Microsoft.Playwright;
using XPing365.Sdk.Core.HeadlessBrowser.Internals;

namespace XPing365.Sdk.Core.HeadlessBrowser;

/// <summary>
/// This class represents a client that can interact with a web application using a headless browser, such as Chromium, 
/// Firefox, or WebKit. It uses the Playwright library to create and control the headless browser instance. 
/// It implements the <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> interfaces to support both 
/// synchronous and asynchronous disposal of the unmanaged resources.
/// </summary>
/// <remarks>
/// A constructor initializes a new instance with the specified browser and context parameters. A browser 
/// <see cref="IBrowser"/> object represents the headless browser instance. It can be obtained from the IPlaywright 
/// interface. A <see cref="BrowserContext"/> object represents the browser context options.
/// </remarks>
/// <param name="browser"></param>
/// <param name="context"></param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class HeadlessBrowserClient(IBrowser browser, BrowserContext context) : IDisposable, IAsyncDisposable
{
    private IBrowser _browser = browser;

    /// <summary>
    /// Gets object that represents the browser context options.
    /// </summary>
    public BrowserContext Context { get; init; } = context;

    /// <summary>
    /// A read-only property that gets the name of the headless browser type, such as “chromium”, “firefox”, or 
    /// “webkit”.
    /// </summary>
    public string Name => _browser.BrowserType.Name;

    /// <summary>
    /// A read-only property that gets the version of the headless browser instance.
    /// </summary>
    public string Version => _browser.Version;

    /// <summary>
    /// An asynchronous method that sends a HTTP request to the specified URL and returns a WebPage object that 
    /// represents the response. The WebPage object provides methods and properties to access and manipulate the web 
    /// page content and functionality.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the web page to request.</param>
    /// <returns>
    /// A Task&lt;WebPage&gt; object that represents the asynchronous operation. The result of the task is a 
    /// <see cref="WebPage"/> object that represents the web page response.
    /// </returns>
    public async Task<WebPage> GetAsync(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));

        var options = new BrowserNewPageOptions
        {
            UserAgent = Context.UserAgent, 
            ViewportSize = Context.ViewportSize
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
