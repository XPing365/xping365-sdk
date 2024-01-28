﻿using XPing365.Sdk.Core.Common;

namespace XPing365.Sdk.Core.Components;

/// <summary>
/// This class is used to store settings for a test execution. It provides a set of properties that can be used to 
/// configure the behavior of the test run, such as the timeout duration, retry behavior, and HTTP redirection behavior.
/// It also includes <see cref="PropertyBag"/> property to store custom settings as a key-value pairs. 
/// </summary>
public sealed class TestSettings
{
    /// <summary>
    /// Default Http request timeout in seconds. 
    /// </summary>
    public const int DefaultHttpRequestTimeoutInSeconds = 30;

    /// <summary>
    /// Gets a property bag which represents the custom properties of the test steps execution.
    /// </summary>
    public PropertyBag PropertyBag { get; } = new();

    /// <summary>
    /// Gets or sets a boolean value which determines whether to retry HTTP requests when they fail. Default is true, 
    /// unless specified differently in <see cref="DefaultForAvailability"/>.
    /// </summary>
    public bool RetryHttpRequestWhenFailed { get; set; } = true;

    /// <summary>
    /// Gets or sets a boolean value which determines whether to follow HTTP redirection responses. Default is true, 
    /// unless specified differently in <see cref="DefaultForAvailability"/>.
    /// </summary>
    public bool FollowHttpRedirectionResponses { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to continue running all tests regardless of their results. Default is 
    /// false.
    /// </summary>
    /// <remarks>
    /// If this property is set to <c>true</c>, all tests will be run regardless of their results. If this property is 
    /// set to <c>false</c>, the testing pipeline will stop running tests when a failure occurs.
    /// </remarks>
    public bool ContinueOnFailure { get; set; }

    /// <summary>
    /// Gets a TestSettings object with default settings for server availability testing.
    /// The returned object has PingDontFragmetOption property set to true and the PingTTLOption property set to 64.
    /// See <see cref="PropertyBagKeys.PingDontFragmetOption"/> and <see cref="PropertyBagKeys.PingTTLOption"/> for more 
    /// information. It also has Http request timeout set to <see cref="DefaultHttpRequestTimeoutInSeconds"/> value.
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
    /// <returns>HTTP method stored in the current test settings. <see cref="HttpMethod.Get"/> is returned
    /// if not specified.
    /// </returns>
    public HttpMethod GetHttpMethod()
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
    /// <returns>HttpContent stored in the current test settings. Null is returned if on <see cref="HttpContent"/> 
    /// defined.
    /// </returns>
    public HttpContent? GetHttpContent()
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
    /// <returns>HTTP request headers or empty dictionary if none specified.</returns>
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