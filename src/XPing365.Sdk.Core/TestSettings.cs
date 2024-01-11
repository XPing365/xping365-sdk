namespace XPing365.Sdk.Core;

/// <summary>
/// This class is used to store settings for a test execution. It provides a set of properties that can be used to 
/// configure the behavior of the test run, such as the timeout duration, retry behavior, and HTTP redirection behavior.
/// </summary>
public sealed class TestSettings
{
    public const int DefaultHttpRequestTimeoutInSeconds = 30;

    /// <summary>
    /// Gets a property bag which represents the custom properties of the test steps execution.
    /// </summary>
    public PropertyBag PropertyBag { get; } = new();

     /// <summary>
    /// Gets or sets a boolean value which determines whether to retry HTTP requests when they fail.
    /// </summary>
    public bool RetryHttpRequestWhenFailed { get; set; } = true;

    /// <summary>
    /// Gets or sets a boolean value which determines whether to follow HTTP redirection responses.
    /// </summary>
    public bool FollowHttpRedirectionResponses { get; set; } = true;

    /// <summary>
    /// Gets a TestSettings object with default settings for server availability testing.
    /// The returned object has PingDontFragmetOption property set to true and the PingTTLOption set property to 64.
    /// <see cref="PropertyBagKeys.PingDontFragmetOption"/> and <see cref="PropertyBagKeys.PingTTLOption"/> for more 
    /// information.
    /// </summary>
    public static TestSettings DefaultForAvailability
    {
        get
        {
            var testSettings = new TestSettings();
            testSettings.PropertyBag.AddOrUpdateProperties(new Dictionary<PropertyBagKey, object>
            {
                { PropertyBagKeys.PingDontFragmetOption, true },
                { PropertyBagKeys.PingTTLOption, 64 },
                { PropertyBagKeys.HttpRequestTimeout, TimeSpan.FromSeconds(DefaultHttpRequestTimeoutInSeconds) }
            });

            return testSettings;
        }
    }

    /// <summary>
    /// Returns HTTP method stored in the current test settings instance.
    /// </summary>
    /// <returns>HTTP method stored in the current test settings. Default is HttpMethod.Get.</returns>
    public HttpMethod GetHttpMethodOrDefault()
    {
        if (PropertyBag.TryGetProperty(PropertyBagKeys.HttpMethod, out object? value) &&
            value is HttpMethod httpMethod)
        {
            return httpMethod;
        }

        return HttpMethod.Get;
    }

    /// <summary>
    /// Returns HTTP content stored in the current test settings instance.
    /// </summary>
    /// <returns>HttpContent stored in the current test settings. Default is null.</returns>
    public HttpContent? GetHttpContentOrDefault()
    {
        if (PropertyBag.TryGetProperty(PropertyBagKeys.HttpContent, out HttpContent? httpContent))
        {
            return httpContent;
        }

        return default;
    }

    /// <summary>
    /// Returns HTTP request headers stored in the current test settings instance.
    /// </summary>
    /// <returns>HTTP request headers. Default is empty dictionary.</returns>
    public IDictionary<string, IEnumerable<string>> GetHttpRequestHeadersOrEmpty()
    {
        if (PropertyBag.TryGetProperty(
            PropertyBagKeys.HttpRequestHeaders, out IDictionary<string, IEnumerable<string>>? bag) && bag != null)
        {
            return bag.ToDictionary();
        }

        return new Dictionary<string, IEnumerable<string>>();
    }
}
