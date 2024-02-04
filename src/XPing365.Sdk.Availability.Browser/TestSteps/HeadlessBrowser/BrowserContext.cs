using XPing365.Sdk.Availability.TestSteps.HeadlessBrowser.Internals;

namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

public class BrowserContext
{
    public static FileInfo BrowserExecutable => new("phantomjs");

    public TimeSpan Timeout { get; set; } = TimeSpan.Zero;

    public string? UserAgent { get; set; }

    internal Script Script { get; set; } = Scripts.LoadHtml;
}
