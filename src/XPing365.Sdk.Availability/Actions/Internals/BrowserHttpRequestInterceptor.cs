using Microsoft.Playwright;
using XPing365.Sdk.Core.Clients.Browser;
using XPing365.Sdk.Core.Clients.Configurations;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestActions.Internals;

internal class BrowserHttpRequestInterceptor(BrowserConfiguration configuration) : IHttpRequestInterceptor
{
    private readonly BrowserConfiguration _configuration = configuration.RequireNotNull(nameof(configuration));

    public async Task HandleAsync(IRoute route)
    {
        var httpContent = _configuration.GetHttpContent();
        var byteArray = httpContent != null ? await httpContent.ReadAsByteArrayAsync().ConfigureAwait(false) : null;

        // Modify the HTTP method here
        var routeOptions = new RouteContinueOptions
        {
            Method = _configuration.GetHttpMethod().Method,
            PostData = byteArray
        };

        await route.ContinueAsync(routeOptions).ConfigureAwait(false);
    }
}
