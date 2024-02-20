namespace XPing365.Sdk.Core.HeadlessBrowser;

/// <summary>
/// This interface defines a method to create a <see cref="HeadlessBrowserClient"/> object that can interact with a web 
/// application using a headless browser, such as Chromium, Firefox, or WebKit. It also implements the IDisposable 
/// interface to support the disposal of unmanaged resources.
/// </summary>
public interface IHeadlessBrowserFactory : IDisposable
{
    /// <summary>
    /// An asynchronous method that creates a <see cref="HeadlessBrowserClient"/> object with the specified browser 
    /// context options.
    /// </summary>
    /// <param name="context">A <see cref="BrowserContext"/> object that represents the browser context options, such 
    /// as browser type, timeout, and user agent.</param>
    /// <returns>
    /// A Task&lt;HeadlessBrowserClient&gt; object that represents the asynchronous operation. The result of 
    /// the task is a <see cref="HeadlessBrowserClient"/> object that can interact with a web application using a 
    /// headless browser.
    /// </returns>
    Task<HeadlessBrowserClient> CreateClientAsync(BrowserContext context);
}
