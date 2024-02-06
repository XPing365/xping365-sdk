using System.Net;
using Microsoft.Playwright;
using XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;
using XPing365.Sdk.Common;

namespace XPing365.Sdk.Availability.Browser.TestSteps.HeadlessBrowser.Internals;

internal sealed partial class WebPageBuilder
{
    private IPage _page = null!;
    private IResponse? _response;

    public WebPageBuilder Build(IPage page)
    {
        _page = page.RequireNotNull(nameof(page));
        return this;
    }

    public WebPageBuilder Build(IResponse? response)
    {
        _response = response.RequireNotNull(nameof(response));
        return this;
    }

    public async Task<WebPage> GetWebPageAsync()
    {
        if (_response == null)
        {
            // Create an error message for the user
            var errorMessage =
                "An error occurred while fetching data from the server. " +
                "The HTTP response content could not be obtained. " +
                "Please check the following:\n" +
                " - The URL of the request is valid and reachable\n" +
                " - The HTTP server is up and running\n" +
                " - The format and content of the data response are correct and expected\n" +
                "If the error persists, please contact the support team for assistance.";

            throw new ArgumentException(errorMessage);
        }

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = (HttpStatusCode)_response.Status,
            Content = new StringContent(await _page.ContentAsync().ConfigureAwait(false))
        };

        foreach (var httpHeader in _response.Headers)
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
}
