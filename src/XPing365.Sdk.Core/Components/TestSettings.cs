using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Net.Http.Headers;
using Microsoft.Playwright;
using XPing365.Sdk.Core.Common;

using Cookie = System.Net.Cookie;

namespace XPing365.Sdk.Core.Components;

/// <summary>
/// This class is used to store settings for a test execution. It provides a set of properties that can be used to 
/// configure the behavior of the test run, such as the timeout duration, retry behavior, and HTTP redirection behavior.
/// It also includes <see cref="PropertyBag"/> property to store custom settings as a key-value pairs. 
/// </summary>
public sealed class TestSettings
{
    private static readonly string[] TextPlainContentType = ["text/plain"];
    private static readonly string[] ApplicationJsonContentType = ["application/json"];
    private static readonly string[] FormUrlEncodedContentType = ["application/x-www-form-urlencoded"];
    private static readonly string[] MultipartContentType = ["multipart/form-data"];
    private static readonly string[] ByteArrayContentType = ["application/octet-stream"];
    private static readonly string[] StreamContentType = ["application/octet-stream"];

    /// <summary>
    /// Default Http request timeout in seconds. 
    /// </summary>
    public const int DefaultHttpRequestTimeoutInSeconds = 30;

    /// <summary>
    /// Gets a property bag which represents the custom properties of the test steps execution.
    /// </summary>
    public PropertyBag<object> PropertyBag { get; } = new();

    /// <summary>
    /// Gets a value that specifies the maximum time to wait for a network request or a browser operation to finish. 
    /// If the time exceeds this value, current operation is terminated.
    /// </summary>
    public TimeSpan HttpRequestTimeout { get; set; } = TimeSpan.FromSeconds(DefaultHttpRequestTimeoutInSeconds);

    /// <summary>
    /// Gets or sets a boolean value which determines whether to retry HTTP requests when they fail. Default is true.
    /// </summary>
    public bool RetryHttpRequestWhenFailed { get; set; } = true;

    /// <summary>
    /// Gets or sets a boolean value which determines whether to follow HTTP redirection responses. Default is true.
    /// </summary>
    public bool FollowHttpRedirectionResponses { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of allowed HTTP redirects. Default is 50.
    /// </summary>
    public int MaxRedirections { get; set; } = 50;

    /// <summary>
    /// Gets or sets a value indicating whether to continue running all tests in the pipline regardless of their 
    /// results. Default is false.
    /// </summary>
    /// <remarks>
    /// If this property is set to <c>true</c>, all tests will be run regardless of their results. If this property is 
    /// set to <c>false</c>, the testing pipeline will stop running tests when a failure occurs.
    /// </remarks>
    public bool ContinueOnFailure { get; set; }

    /// <summary>
    /// Gets or sets a value which specifies the browser type to use for the web tests. It can be one of the values from 
    /// the BrowserType enum in the Playwright library, such as “chromium”, “firefox”, or “webkit”. The default value 
    /// is “chromium”.
    /// </summary>
    /// <remarks>
    /// The BrowserType property is used by to create a new browser instance of the specified type. 
    /// For example, you can set this property as follows
    /// <code>
    /// var settings = new TestSettings
    /// {
    ///     BrowserType = BrowserType.Firefox;
    /// }
    /// </code>
    /// </remarks>
    public string BrowserType { get; set; } = "chromium";

    /// <summary>
    /// Gets a TestSettings object with default settings for request clients. The returned object has 
    /// PingDontFragmetOption property set to true and the PingTTLOption property set to 64.
    /// See <see cref="PropertyBagKeys.PingDontFragmetOption"/> and <see cref="PropertyBagKeys.PingTTLOption"/> for more 
    /// information.
    /// </summary>
    public static TestSettings Default
    {
        get
        {
            var testSettings = new TestSettings();
            testSettings.PropertyBag.AddOrUpdateProperties(new Dictionary<PropertyBagKey, object>
            {
                { PropertyBagKeys.PingDontFragmetOption, true },
                { PropertyBagKeys.PingTTLOption, 64 }
            });

            return testSettings;
        }
    }

    /// <summary>
    /// Stores the specified HTTP request headers for subsequent use in requests made by the HttpClient and headless 
    /// browser. This method ensures that all prepared headers are applied to requests, facilitating consistent 
    /// communication with web servers.
    /// </summary>
    /// <param name="headers">
    /// A dictionary of header names and their corresponding values to be applied to outgoing requests.
    /// </param>
    /// <example>
    /// <code>
    /// var httpRequestHeaders = new Dictionary&lt;string, IEnumerable&lt;string&gt;&gt;
    /// {
    ///     { HeaderNames.UserAgent, ["Chrome/51.0.2704.64 Safari/537.36"] }
    /// };
    /// var testSettings = TestSettings.Default;
    /// testSettings.SetHttpRequestHeaders(httpRequestHeaders);
    /// </code>
    /// </example>
    public void SetHttpRequestHeaders(IDictionary<string, IEnumerable<string>> headers)
    {
        ArgumentNullException.ThrowIfNull(headers, nameof(headers));

        PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpRequestHeaders, headers);
    }

    /// <summary>
    /// Returns HTTP request headers stored in the current test settings instance.
    /// </summary>
    /// <returns>HTTP request headers or empty dictionary if none specified.</returns>
    public IDictionary<string, IEnumerable<string>> GetHttpRequestHeaders()
    {
        if (PropertyBag.TryGetProperty(
            PropertyBagKeys.HttpRequestHeaders, out IDictionary<string, IEnumerable<string>>? bag) && bag != null)
        {
            return bag;
        }

        var emptyHttpRequstHeaders = new Dictionary<string, IEnumerable<string>>();
        PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpRequestHeaders, emptyHttpRequstHeaders);

        return emptyHttpRequstHeaders;
    }

    /// <summary>
    /// Clears the HTTP request headers stored in the current test settings instance.
    /// </summary>
    public void ClearHttpRequestHeaders()
    {
        PropertyBag.Clear(PropertyBagKeys.HttpRequestHeaders);
    }

    /// <summary>
    /// Stores the specified HTTP method in the test settings for use in subsequent HTTP requests.
    /// </summary>
    /// <param name="httpMethod">The HTTP method to be used for requests.</param>
    public void SetHttpMethod(HttpMethod httpMethod)
    {
        PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpMethod, httpMethod);
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
    /// Stores the specified HttpContent and optionally sets the corresponding content-type header in the current 
    /// TestSettings instance.
    /// </summary>
    /// <param name="httpContent">The HttpContent to store.</param>
    /// <param name="setContentHeaders">
    /// If true, automatically sets the `content-type` header based on the type of HttpContent provided.
    /// </param>
    /// <remarks>
    /// <note>
    /// It is important to note that invoking this method does not alter the HTTP method of the requests; specifically, 
    /// it does not set the HTTP method to POST. If required, the HTTP method should be set independently of the 
    /// HttpContent.
    /// </note>
    /// This content will be used for all subsequent HTTP requests made through the HttpClient and headless browser.
    /// Additionally, if <paramref name="setContentHeaders"/> is set to <c>true</c>, the 'content-type' header will be 
    /// automatically set based on the <paramref name="httpContent"/> provided:
    /// <list type="bullet">
    ///     <item>
    ///         <description><c>StringContent</c>: <c>"text/plain"</c></description>
    ///     </item>
    ///     <item>
    ///         <description><c>JsonContent</c>: <c>"application/json"</c></description>
    ///     </item>
    ///     <item>
    ///         <description><c>FormUrlEncodedContent</c>: <c>"application/x-www-form-urlencoded"</c></description>
    ///     </item>
    ///     <item>
    ///         <description><c>MultipartFormDataContent</c>: <c>"multipart/form-data"</c></description>
    ///     </item>
    ///     <item>
    ///         <description><c>ByteArrayContent</c>: <c>"application/octet-stream"</c></description>
    ///     </item>
    ///     <item>
    ///         <description><c>StreamContent</c>: <c>"application/octet-stream"</c></description>
    ///     </item>
    /// </list>
    /// <exqample>
    /// <code>
    /// HttpContent httpContent = JsonContent.Create("{\"name\":\"John\", \"age\":30, \"car\":null}");
    /// var testSettings = TestSettings.Default;
    /// testSettings.SetHttpContent(httpContent);
    /// </code>
    /// </exqample>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Throws when httpContent is null.</exception>
    public void SetHttpContent(HttpContent httpContent, bool setContentHeaders = true)
    {
        ArgumentNullException.ThrowIfNull(httpContent, nameof(httpContent));

        PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpContent, httpContent);

        if (setContentHeaders)
        {
            IDictionary<string, IEnumerable<string>> httpHeaders = GetHttpRequestHeaders();

            switch (httpContent)
            {
                case StringContent _:
                    httpHeaders.TryAdd(HeaderNames.ContentType, TextPlainContentType);
                    break;
                case JsonContent _:
                    httpHeaders.TryAdd(HeaderNames.ContentType, ApplicationJsonContentType);
                    break;
                case FormUrlEncodedContent _:
                    httpHeaders.TryAdd(HeaderNames.ContentType, FormUrlEncodedContentType);
                    break;
                case MultipartFormDataContent _:
                    httpHeaders.TryAdd(HeaderNames.ContentType, MultipartContentType);
                    break;
                case ByteArrayContent _:
                    httpHeaders.TryAdd(HeaderNames.ContentType, ByteArrayContentType);
                    break;
                case StreamContent _:
                    httpHeaders[HeaderNames.ContentType] = StreamContentType;
                    break;
                // Add other content types as needed
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Returns HTTP content stored in the current test settings instance.
    /// </summary>
    /// <returns>HttpContent stored in the current test settings. Null is returned if no <see cref="HttpContent"/> 
    /// defined.
    /// </returns>
    public HttpContent? GetHttpContent()
    {
        if (PropertyBag.TryGetProperty(PropertyBagKeys.HttpContent, out HttpContent? httpContent))
        {
            return httpContent;
        }

        return null;
    }

    /// <summary>
    /// Clears the HTTP content stored in the current test settings instance.
    /// </summary>
    /// <remarks>
    /// Please note this method does not update the content-type header.
    /// </remarks>
    public void ClearHttpContent()
    {
        PropertyBag.Clear(PropertyBagKeys.HttpContent);
    }

    /// <summary>
    /// Stores a cookie in the current test settings instance.
    /// </summary>
    /// <param name="cookie">The cookie to be added.</param>
    /// <exception cref="ArgumentNullException">Thrown when cookie is null.</exception>
    public void AddCookie(Cookie cookie)
    {
        ArgumentNullException.ThrowIfNull(cookie, nameof(cookie));

        IDictionary<string, IEnumerable<string>> httpRequestHeaders = GetHttpRequestHeaders();

        // Convert the Cookie object to a string representation
        string cookieString = $"{cookie.Name}={cookie.Value}";

        // Check if the 'Cookie' header already exists
        if (httpRequestHeaders.TryGetValue(HeaderNames.Cookie, out var existingCookies))
        {
            // Append the new cookie to the existing 'Cookie' header
            httpRequestHeaders[HeaderNames.Cookie] = new List<string>(existingCookies) { cookieString };
        }
        else
        {
            // Add a new 'Cookie' header with the new cookie
            httpRequestHeaders[HeaderNames.Cookie] = [cookieString];
        }
    }

    /// <summary>
    /// Adds a collection of cookies to the current test settings.
    /// This method iterates through each cookie in the provided CookieCollection
    /// and adds them individually using the AddCookie method.
    /// </summary>
    /// <param name="cookieCollection">The collection of cookies to be added. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when cookieCollection is null.</exception>
    public void AddCookies(CookieCollection cookieCollection)
    {
        ArgumentNullException.ThrowIfNull(cookieCollection, nameof(cookieCollection));

        foreach (Cookie cookie in cookieCollection.Cast<Cookie>())
        {
            AddCookie(cookie);
        }
    }

    /// <summary>
    /// Returns a list of cookies stored in the current test settings instance.
    /// </summary>
    /// <returns>A CookieCollection containing all the cookies from the test settings.</returns>
    public CookieCollection GetCookies()
    {
        IDictionary<string, IEnumerable<string>> httpRequestHeaders = GetHttpRequestHeaders();

        var cookieCollection = new CookieCollection();

        if (httpRequestHeaders.TryGetValue(HeaderNames.Cookie, out var cookies))
        {
            foreach (var cookie in cookies)
            {
                var cookieParts = cookie.Trim().Split('=');

                if (cookieParts.Length == 2)
                {
                    cookieCollection.Add(new Cookie(cookieParts[0].Trim(), cookieParts[1].Trim()));
                }
            }
        }

        return cookieCollection;
    }

    /// <summary>
    /// Clears all the cookies stored in the current test settings instance.
    /// </summary>
    public void ClearCookies()
    {
        IDictionary<string, IEnumerable<string>> httpRequestHeaders = GetHttpRequestHeaders();
        httpRequestHeaders.Remove(HeaderNames.Cookie);
    }

    /// <summary>
    /// Sets the credentials for HTTP request headers using basic authentication.
    /// <note type="important">
    /// The credentials are encoded in Base64, which does not provide encryption. Base64 is a binary-to-text encoding 
    /// scheme that is easily reversible. Therefore, the credentials are sent in plain text.
    /// </note>
    /// </summary>
    /// <param name="credential">The NetworkCredential object containing the user's credentials.</param>
    public void SetCredentials(NetworkCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential, nameof(credential));
        
        IDictionary<string, IEnumerable<string>> httpRequestHeaders = GetHttpRequestHeaders();

        // Set the 'Authorization' header to 'Basic {encoded-credentials}' to authenticate the request.
        string basicAuthenticationValue =
            Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{credential.UserName}:{credential.Password}"));

        httpRequestHeaders[HeaderNames.Authorization] =
            [new AuthenticationHeaderValue("Basic", basicAuthenticationValue).ToString()];
    }

    /// <summary>
    /// Clears the credentials stored in current test settings instance.
    /// </summary>
    public void ClearCredentials()
    {
        IDictionary<string, IEnumerable<string>> httpRequestHeaders = GetHttpRequestHeaders();
        httpRequestHeaders.Remove(HeaderNames.Authorization);
    }

    /// <summary>
    /// Stores the geolocation coordinates in the test settings instance for use with a headless browser.
    /// </summary>
    /// <param name="geolocation">The Geolocation object containing latitude and longitude coordinates.</param>
    /// <remarks>
    /// <note>
    /// Note that this method is not applicable for setting geolocation in an HttpClient instance, as HttpClient does 
    /// not inherently support geolocation.
    /// </note>
    /// This method updates the PropertyBag with the provided Geolocation object. If the Geolocation key already exists,
    /// it will be updated with the new coordinates; otherwise, a new entry will be added. This is particularly useful
    /// for setting up location-based scenarios in automated tests using a headless browser. 
    /// </remarks>
    public void SetGeolocation(Geolocation geolocation)
    {
        PropertyBag.AddOrUpdateProperty(PropertyBagKeys.Geolocation, geolocation);
    }

    /// <summary>
    /// Retrieves the geolocation coordinates from the test settings instance.
    /// </summary>
    /// <returns>
    /// A Geolocation object containing latitude and longitude coordinates if found; otherwise, null.
    /// </returns>
    /// <remarks>
    /// This method attempts to retrieve the Geolocation object associated with the current test settings.
    /// If the PropertyBag contains the Geolocation key, the method will return the corresponding Geolocation object;
    /// otherwise, it will return null, indicating that no geolocation has been set.
    /// </remarks>
    public Geolocation? GetGeolocation()
    {
        PropertyBag.TryGetProperty<Geolocation>(PropertyBagKeys.Geolocation, out var geolocation);
        return geolocation;
    }

    /// <summary>
    /// Removes the geolocation information from the test settings instance.
    /// </summary>
    /// <remarks>
    /// This method clears any stored geolocation data associated with the Geolocation key in the PropertyBag.
    /// It is useful for resetting the test environment or ensuring that no location data persists between tests.
    /// </remarks>
    public void ClearGeolocation()
    {
        PropertyBag.Clear(PropertyBagKeys.Geolocation);
    }
}
