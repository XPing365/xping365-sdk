using System.Runtime.InteropServices;
using XPing365.Sdk.Availability.TestSteps.HeadlessBrowser.Internals;

namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

public class BrowserContext
{
    public static FileInfo? BrowserExecutable
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new FileInfo("Browser/win-x64/phantomjs.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new FileInfo("Browser/linux-x64/phantomjs");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new FileInfo("Browser/macosx/phantomjs");
            }

            return null;
        }
    }

    public TimeSpan Timeout { get; set; } = TimeSpan.Zero;

    public string? UserAgent { get; set; }

    internal Script Script { get; set; } = Scripts.LoadHtml;
}
