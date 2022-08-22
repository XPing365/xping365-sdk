using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using XPing365.Core.DataSource;

namespace XPing365.Core.DataRetrieval
{
    public class WebBrowserRetrieval : IWebDataRetrieval
    {
        private readonly Regex OutputRegex = new("---OUTPUT---[\\n|\\r]+ResponseCode: ([0-9]{3})[\\n|\\r]+RequestStartTime: ([0-9]{13,})[\\n|\\r]+RequestEndTime: ([0-9]{13,})", RegexOptions.Compiled);
        private readonly Regex HtmlRegex = new("---HTML---[\\n|\\r]+((.|[\\n|\\r]+)*)", RegexOptions.Compiled);

        private readonly ServiceConfigurator configurator;

        public WebBrowserRetrieval(ServiceConfigurator configurator)
        {
            this.configurator = configurator;
        }

        public async Task<T> GetFromHtmlAsync<T>(string url, TimeSpan timeout) where T : HtmlSource, new()
        {
            StringBuilder output = new();
            string errMsg = string.Empty;

            int exitCode = await RunProcessAsync(
                this.configurator.WebBrowserSection.Path, 
                $".\\DataRetrieval\\Scripts\\LoadHtml.js {url} {timeout.Milliseconds} {this.GetUserAgent()}",
                (s, ea) => output.AppendLine(ea.Data ?? string.Empty),
                (s, ea) => errMsg = ea.Data ?? string.Empty);

            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Process `{Path.GetFileName(this.configurator.WebBrowserSection.Path)}` exited with code {exitCode}. {errMsg}");
            }

            T dataSource = this.ParseOutput<T>(url, output.ToString());
            return dataSource;
        }

        public T ParseOutput<T>(string url, string outputResult) where T : HtmlSource, new()
        {
            DateTime requestStartTime = DateTime.UtcNow;
            DateTime requestEndTime = requestStartTime;
            HttpStatusCode statusCode = 0;
            string html = string.Empty;

            Match matchParams = this.OutputRegex.Match(outputResult);

            if (matchParams.Success)
            {
                statusCode = (HttpStatusCode)int.Parse(matchParams.Groups[1].Value);

                requestStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                       .AddMilliseconds(long.Parse(matchParams.Groups[2].Value));
                requestEndTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                     .AddMilliseconds(long.Parse(matchParams.Groups[3].Value));
            }

            Match matchHtml = this.HtmlRegex.Match(outputResult);

            if (matchHtml.Success)
            {
                html = matchHtml.Groups[1].Value;
            }

            T dataSource = new()
            {
                Url = url,
                Html = html,
                RequestStartTime = requestStartTime,
                RequestEndTime = requestEndTime,
                ResponseCode = statusCode,
                IsSuccessResponseCode = (int)statusCode >= 200 && (int)statusCode < 300,
                ResponseSizeInBytes = html.Length * sizeof(char)
            };

            return dataSource;
        }

        public static async Task<int> RunProcessAsync(string fileName, string args, DataReceivedEventHandler dataReceived, DataReceivedEventHandler? errorReceived)
        {
            using var process = new Process
            {
                StartInfo =
                {
                    FileName = fileName, Arguments = args,
                    UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardOutput = true, RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            return await RunProcessAsync(process, dataReceived, errorReceived).ConfigureAwait(false);
        }

        private static Task<int> RunProcessAsync(Process process, DataReceivedEventHandler dataReceived, DataReceivedEventHandler? errorReceived)
        {
            var tcs = new TaskCompletionSource<int>();

            process.Exited += (s, ea) => tcs.SetResult(process.ExitCode);
            process.OutputDataReceived += dataReceived;
            process.ErrorDataReceived += errorReceived;

            bool started = process.Start();
            if (!started)
            {
                //you may allow for the process to be re-used (started = false) 
                //but I'm not sure about the guarantees of the Exited event in such a case
                throw new InvalidOperationException("Could not start process: " + process);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }

        private string GetUserAgent()
        {
            if (this.configurator.HttpRequestSection.Headers != null)
            {
                if (this.configurator.HttpRequestSection.Headers.ContainsKey("UserAgent"))
                {
                    string userAgent = this.configurator.HttpRequestSection.Headers["UserAgent"];
                    return userAgent;
                }
            }

            return string.Empty;
        }
    }
}
