using System.Drawing;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

namespace XPing365.Sdk.Availability.Browser.TestSteps.HeadlessBrowser.Internals;

internal sealed partial class WebPageBuilder
{
    private const int MinErrorLength = 2;

    [GeneratedRegex("[Ee]rror:(.*)")]
    private static partial Regex ErrorRegex();

    [GeneratedRegex("(?<=---OUTPUT---\r?\n)ResponseCode: ?([0-9]{3})[\r?\n]+RequestStartTime: ?([0-9]{13,})[\r?\n]+RequestEndTime: ?([0-9]{13,})")]
    private static partial Regex OutputRegex();

    [GeneratedRegex("(?<=---HEADERS---\r?\n)((.|\r?\n)*?)(?=\r?\n---HEADERS---)")]
    private static partial Regex ResponseHeaders();

    //[GeneratedRegex("---HTML---[\\n|\\r]+((.|[\\n|\\r]+)*)")]
    [GeneratedRegex("(?<=---HTML---\r?\n)((.|\r?\n)*)")]
    private static partial Regex HtmlRegex();

    private readonly StringBuilder _dataBuffer = new();
    private readonly StringBuilder _errorBuffer = new();

    public WebPageBuilder BuildData(string? data)
    {
        _dataBuffer.AppendLine(data);
        return this;
    }

    public WebPageBuilder BuildError(string? error)
    {
        _errorBuffer.AppendLine(error);
        return this;
    }

    public WebPage GetWebPage(int browserExitCode)
    {
        if (browserExitCode != 0 || _errorBuffer.Length > MinErrorLength)
        {
            // Create an error message for the user
            var errorMessage = "An error occurred while trying to access the data from the headless browser. " +
                "The browser exited with an error code: " + browserExitCode + ". Please check the following:\n" +
                "- The headless browser is installed and configured correctly\n" +
                "- The URL of the data source is valid and reachable\n" +
                "- The headless browser options and arguments are compatible with the data source\n" +
                "- The network connection is stable and reliable\n" +
                "If the error persists, please contact the support team for assistance.";

            throw new ArgumentException(errorMessage,
                new InvalidOperationException(GetErrorMessage(_errorBuffer.ToString())));
        }

        string data = _dataBuffer.ToString();

        if (data.ToUpperInvariant().StartsWith("ERROR: UNABLE TO ACCESS NETWORK", StringComparison.InvariantCulture))
        {
            // Create an error message for the user
            var errorMessage = "Sorry, we encountered a network error while loading the web page.\n" +
                "This could be caused by various factors, such as:\n" +
                "- The web page is not available or reachable\n" +
                "- The network connection is unstable or interrupted\n" +
                "- The headless browser settings are not configured properly\n" +
                "Please check the following:\n" +
                "- The URL of the web page is valid and correct\n" +
                "- The timeout value for the test session is sufficient and not exceeded by the request duration\n" +
                "- The network connection is stable and reliable\n" +
                "If the problem persists, please contact us for further assistance.";
            throw new ArgumentException(errorMessage,
                new InvalidOperationException(GetErrorMessage(_errorBuffer.ToString())));
        }

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = GetStatusCode(data),
            Content = GetContent(data)
        };

        foreach (var httpHeader in GetHttpResponseHeaders(data))
        {
            if (httpHeader.Key
                .ToUpperInvariant()
                .StartsWith("CONTENT", StringComparison.InvariantCulture))
            {
                if (responseMessage.Content.Headers.Contains(httpHeader.Key))
                {
                    responseMessage.Content.Headers.Remove(httpHeader.Key);
                }

                responseMessage.Content.Headers.TryAddWithoutValidation(httpHeader.Key, httpHeader.Value);
            }
            else
            {
                responseMessage.Headers.TryAddWithoutValidation(httpHeader.Key, httpHeader.Value);
            }
        }

        return new WebPage(responseMessage);
    }

    private static HttpStatusCode GetStatusCode(string data)
    {
        Match matchParams = OutputRegex().Match(data);

        if (!matchParams.Success)
        {
            // Create an error message for the user
            var errorMessage =
                "An error occurred while parsing the data response. The HTTP status code could not be obtained. " +
                "Please check the following:\n" +
                "- The URL of the request is valid and reachable\n" +
                "- The format and content of the data response are correct and expected\n" +
                "- The network connection is stable and reliable\n" +
                "If the error persists, please contact the support team for assistance.";

            throw new ArgumentException(errorMessage, nameof(data));
        }

        var statusCode = (HttpStatusCode)int.Parse(matchParams.Groups[1].Value, CultureInfo.InvariantCulture);
        return statusCode;
    }

    private static StringContent GetContent(string data)
    {
        Match matchHtml = HtmlRegex().Match(data);

        if (!matchHtml.Success)
        {
            // Create an error message for the user
            var errorMessage =
                "An error occurred while parsing the data response. The HTTP response content could not be obtained. " +
                "Please check the following:\n" +
                "- The URL of the request is valid and reachable\n" +
                "- The format and content of the data response are correct and expected\n" +
                "- The network connection is stable and reliable\n" +
                "If the error persists, please contact the support team for assistance.";

            throw new ArgumentException(errorMessage, nameof(data));
        }

        string content = matchHtml.Groups[1].Value;
        var stringContent = new StringContent(content);

        return stringContent;
    }

    private static Dictionary<string, string> GetHttpResponseHeaders(string data)
    {
        Match matchHeaders = ResponseHeaders().Match(data);

        if (!matchHeaders.Success)
        {
            // Create an error message for the user
            var errorMessage =
                "An error occurred while parsing the data response. The HTTP response headers could not be obtained. " +
                "Please check the following:\n" +
                "- The URL of the request is valid and reachable\n" +
                "- The format and content of the data response are correct and expected\n" +
                "- The network connection is stable and reliable\n" +
                "If the error persists, please contact the support team for assistance.";

            throw new ArgumentException(errorMessage, nameof(data));
        }

        string content = matchHeaders.Groups[1].Value;

        // Create a new dictionary<string, string> to store http headers
        var dictionary = new Dictionary<string, string>();

        // Split the text by newline character to get each header line
        var lines = content.Split('\n');

        // Loop through each line
        foreach (var line in lines)
        {
            // Split the line by colon character to get the key and the value
            var parts = line.Split(':');

            // Check if the line has exactly two parts
            if (parts.Length == 2)
            {
                // Trim any whitespace from the key and the value
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                // Add the key-value pair to the dictionary
                dictionary.Add(key, value);
            }
        }

        return dictionary;
    }

    private static string? GetErrorMessage(string? data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            Match matchHtml = HtmlRegex().Match(data);

            if (!matchHtml.Success)
            {
                // Only check for errors if there is no html output.
                Match matchError = ErrorRegex().Match(data);

                if (matchError.Success)
                {
                    var errorMessage = matchError.Groups[1].Value.Trim();
                    return errorMessage;
                }
            }
        }

        return null;
    }
}
