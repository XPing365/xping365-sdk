namespace XPing365.Sdk.Core.Common;

/// <summary>
/// Contains commonly used user agent strings for desktop and mobile browsers.
/// </summary>
/// <remarks>
/// The UserAgent class provides a convenient way to access predefined user agent strings that can be used to simulate 
/// different browsers in HTTP requests. This can be particularly useful when performing automated testing where 
/// mimicking a specific browser is required.
/// </remarks>
public static class UserAgent
{
    /// <summary>
    /// Represents a user agent string for Google Chrome on desktop.
    /// </summary>
    public static readonly string ChromeDesktop = 
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/58.0.3029.110 Safari/537.3";

    /// <summary>
    /// Represents a user agent string for Mozilla Firefox on desktop.
    /// </summary>
    public static readonly string FirefoxFesktop = 
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:53.0) Gecko/20100101 Firefox/53.0";

    /// <summary>
    /// Represents a user agent string for Microsoft Edge on desktop.
    /// </summary>
    public static readonly string EdgeDesktop = 
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/42.0.2311.135 Safari/537.36 Edge/12.246";

    /// <summary>
    /// Represents a user agent string for Safari on mobile devices.
    /// </summary>
    public static readonly string SafariMobile = 
        "Mozilla/5.0 (iPhone; CPU iPhone OS 10_3_1 like Mac OS X) AppleWebKit/603.1.30 (KHTML, like Gecko) " +
        "Version/10.0 Mobile/14E304 Safari/602.1";

    /// <summary>
    /// Represents a user agent string for Google Chrome on mobile devices.
    /// </summary>    
    public static readonly string ChromeMobile =
        "Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/58.0.3029.110 Mobile Safari/537.36";

    /// <summary>
    /// Represents a user agent string for Mozilla Firefox on mobile devices.
    /// </summary>
    public static readonly string FirefoxFireMobile = 
        "Mozilla/5.0 (Android 7.0; Mobile; rv:53.0) Gecko/53.0 Firefox/53.0";
}
