using System.Diagnostics;
using System.Web;
using XPing365.Sdk.Availability.Browser.TestSteps.HeadlessBrowser.Internals;

namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser.Internals;

internal sealed class PhantomJsBrowserClient(BrowserContext context) : HeadlessBrowserClient(context)
{
    private readonly Process _process = new();
    private readonly WebPageBuilder _builder = new();

    public override async Task<WebPage> GetAsync(
        Uri url,
        CancellationToken cancellationToken = default)
    {
        _process.StartInfo.FileName = BrowserContext.BrowserExecutable?.FullName;
        _process.StartInfo.Arguments = GetProcessArguments(url, Context);
        _process.StartInfo.UseShellExecute = false;
        _process.StartInfo.CreateNoWindow = true;
        _process.StartInfo.RedirectStandardOutput = true;
        _process.StartInfo.RedirectStandardError = true;
        _process.EnableRaisingEvents = true;

        int exitCode = await RunProcessAsync(
            dataReceived: (object sender, DataReceivedEventArgs dataReceivedArgs) => 
                _builder.BuildData(dataReceivedArgs.Data),
            errorReceived: (object sender, DataReceivedEventArgs dataReceivedArgs) =>
                _builder.BuildError(dataReceivedArgs.Data),
            cancellationToken).ConfigureAwait(false);

        return _builder.GetWebPage(browserExitCode: exitCode);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _process?.Dispose();
    }

    private static string GetProcessArguments(Uri requestUri, BrowserContext context)
    {
        return $"{context.Script.Path.FullName} " +                         // arg[0]
            $"{requestUri.AbsoluteUri} " +                                  // arg[1]
            $"{context.Timeout.TotalMilliseconds} " +                       // arg[2]
            $"\"{HttpUtility.JavaScriptStringEncode(context.UserAgent)}\""; // arg[3]
    }

    private async Task<int> RunProcessAsync(
        DataReceivedEventHandler dataReceived,
        DataReceivedEventHandler? errorReceived,
        CancellationToken cancellationToken)
    {
        _process.OutputDataReceived += dataReceived;
        _process.ErrorDataReceived += errorReceived;
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        await _process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        return _process.ExitCode;
    }
}
