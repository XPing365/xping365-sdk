using Microsoft.Playwright;

namespace XPing365.Sdk.Core.HeadlessBrowser;

/// <summary>
/// This class represents the browser context options that are used to create a new headless browser.
/// </summary>
public class BrowserContext
{
    /// <summary>
    /// Gets or sets a string that specifies the browser type to use. It can be one of the values from the 
    /// <see cref="BrowserType"/> enum, such as Chromium, Firefox, or WebKit. 
    /// The default value is BrowserType.Chromium.
    /// </summary>
    public string Type { get; set; } = BrowserType.Chromium;

    /// <summary>
    /// Gets or sets a TimeSpan struct that specifies the maximum time to wait for browser operations.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// A string that specifies the user agent to use in the headless browser context. It can be any valid user agent 
    /// string, such as “Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.5790.75 Safari/537.36”. 
    /// The default value is null, which means the default user agent of the browser type will be used.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Emulates consistent viewport for each page. Defaults to an 1280x720 viewport.
    /// Learn more about <a href="https://playwright.dev/dotnet/docs/emulation#viewport">viewport emulation</a>.
    /// </summary>
    public ViewportSize? ViewportSize { get; set; }
}
