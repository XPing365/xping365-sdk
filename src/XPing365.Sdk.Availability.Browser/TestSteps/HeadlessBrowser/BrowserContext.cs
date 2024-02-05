using Microsoft.Playwright;

namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

public class BrowserContext
{
    public string Type { get; set; } = BrowserType.Chromium;

    public TimeSpan Timeout { get; set; } = TimeSpan.Zero;

    public string? UserAgent { get; set; }
}
