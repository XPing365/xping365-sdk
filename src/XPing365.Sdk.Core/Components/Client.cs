namespace XPing365.Sdk.Core.Components;

/// <summary>
/// An enum that represents the type of client used to send HTTP requests
/// </summary>
public enum Client
{
    /// <summary>
    /// A client that uses the <see cref="System.Net.Http.HttpClient"/> class
    /// </summary>
    HttpClient,

    /// <summary>
    /// A client that uses a headless browser from Playwright
    /// </summary>
    /// <remarks>
    /// Playwright is a library that allows controlling Chromium, WebKit, or Firefox browsers
    /// </remarks>
    HeadlessBrowser
}
