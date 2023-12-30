using Microsoft.Net.Http.Headers;

namespace XPing365.Core.Extensions;

public static class TestSettingsExtensions
{
    public static HttpMethod GetHttpMethodOrDefault(this TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.PropertyBag.TryGetProperty(PropertyBagKeys.HttpMethod, out object? value) && 
            value is HttpMethod httpMethod)
        {
            return httpMethod;
        }

        return HttpMethod.Get;
    }

    public static HttpContent? GetHttpContentOrDefault(this TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.PropertyBag.TryGetProperty(PropertyBagKeys.HttpContent, out HttpContent? httpContent))
        {
            return httpContent;
        }

        return default;
    }

    public static IDictionary<string, IEnumerable<string>> GetHttpRequestHeadersOrEmpty(
        this TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Dictionary<string, IEnumerable<string>> httpRequestHeaders = [];

        if (settings.PropertyBag.TryGetProperty(
            PropertyBagKeys.HttpHeaders, out IDictionary<string, IEnumerable<string>>? bag) && bag != null)
        {
            foreach (var kvp in bag)
            {
                httpRequestHeaders.Add(kvp.Key, kvp.Value);
            }
        }

        if (settings.PropertyBag.TryGetProperty(PropertyBagKeys.UserAgent, out string? userAgent) && 
            !string.IsNullOrEmpty(userAgent))
        {
            // Throw when key already defined
            httpRequestHeaders.Add(HeaderNames.UserAgent, new string[] { userAgent });
        }

        return httpRequestHeaders;
    }

    public static bool GetHttpRetryOrDefault(this TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.PropertyBag.TryGetProperty(PropertyBagKeys.HttpRetry, out bool retry))
        {
            return retry;
        }

        return default;
    }

    public static bool GetHttpFollowRedirectOrDefault(this TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.PropertyBag.TryGetProperty(PropertyBagKeys.HttpFollowRedirect, out bool followRedirect))
        {
            return followRedirect;
        }

        return default;
    }
}
