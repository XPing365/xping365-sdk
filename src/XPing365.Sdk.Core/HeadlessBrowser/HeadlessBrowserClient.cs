using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Microsoft.Net.Http.Headers;
using Microsoft.Playwright;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.HeadlessBrowser.Internals;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.HeadlessBrowser;

/// <summary>
/// This class represents a client that can interact with a web application using a headless browser, such as Chromium, 
/// Firefox, or WebKit. It uses the Playwright library to create and control the headless browser instance. 
/// It implements the <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> interfaces to support both 
/// synchronous and asynchronous disposal of the unmanaged resources.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class HeadlessBrowserClient : IDisposable, IAsyncDisposable
{
    private IBrowser _browser;

    /// <summary>
    /// Initializes a new instance with the specified browser and settings parameters. A browser 
    /// <see cref="IBrowser"/> object represents the headless browser instance. It can be obtained from the IPlaywright 
    /// interface.
    /// </summary>
    /// <param name="browser">The headless browser instance.</param>
    /// <param name="settings">The test settings instance.</param>
    public HeadlessBrowserClient(IBrowser browser, TestSettings settings)
    {
        _browser = browser.RequireNotNull(nameof(browser));
        Settings = settings.RequireNotNull(nameof(settings));
    }

    /// <summary>
    /// Gets object that represents the test settings options.
    /// </summary>
    public TestSettings Settings { get; init; }

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
    /// <param name="onHttpRedirection">
    /// An optional action that will be invoked when an HTTP redirection response is received.
    /// </param>
    /// <returns>
    /// A Task&lt;WebPage&gt; object that represents the asynchronous operation. The result of the task is a 
    /// <see cref="WebPage"/> object that represents the web page response.
    /// </returns>
    public async Task<WebPage> GetAsync(Uri url, Action<IResponse>? onHttpRedirection = null)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));

        Settings.GetHttpRequestHeaders().TryGetValue(HeaderNames.UserAgent, out var userAgentHeader);

        var options = new BrowserNewPageOptions
        {
            UserAgent = userAgentHeader?.FirstOrDefault(),
            ExtraHTTPHeaders = ConvertDictionary(Settings.GetHttpRequestHeaders()),
            Geolocation = Settings.GetGeolocation()
        };

        var page = await _browser.NewPageAsync(options).ConfigureAwait(false);

        // Listen for all responses
        page.Response += (_, response) =>
        {
            if (response.Status >= 300 && response.Status <= 399)
            {
                onHttpRedirection?.Invoke(response);
            }
        };

        // Intercept network requests
        await page.RouteAsync("**/*", async (route) =>
        {
            var httpContent = Settings.GetHttpContent();
            var byteArray = httpContent != null ? await httpContent.ReadAsByteArrayAsync().ConfigureAwait(false) : null;

            // Modify the HTTP method here
            var routeOptions = new RouteContinueOptions
            {
                Method = Settings.GetHttpMethod().Method,
                PostData = byteArray
            };
            await route.ContinueAsync(routeOptions).ConfigureAwait(false);
        }).ConfigureAwait(false);

        var response = await page.GotoAsync(
                url: url.AbsoluteUri,
                options: new PageGotoOptions { Timeout = (float)Settings.HttpRequestTimeout.TotalMilliseconds })
            .ConfigureAwait(false);

        var webpage = await new WebPageBuilder()
            .Build(page)
            .Build(response)
            .GetWebPageAsync()
            .ConfigureAwait(false);

        return webpage;
    }

    /// <summary>
    /// Releases the resources used by the HeadlessBrowserClient instance.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously releases the resources used by the headless browser client.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the headless browser client and optionally releases the managed 
    /// resources.
    /// </summary>
    /// <param name="disposing">A flag indicating whether to release the managed resources.</param>
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

    /// <summary>
    /// Asynchronously performs the core logic of disposing the headless browser client.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync().ConfigureAwait(false);
        }

        _browser = null!;
    }

    private static IEnumerable<KeyValuePair<string, string>> ConvertDictionary(
        IDictionary<string, IEnumerable<string>> dictionary)
    {
        foreach (var pair in dictionary)
        {
            foreach (var value in pair.Value)
            {
                yield return new KeyValuePair<string, string>(pair.Key, value);
            }
        }
    }


    private string GetDebuggerDisplay() => Name;
}
