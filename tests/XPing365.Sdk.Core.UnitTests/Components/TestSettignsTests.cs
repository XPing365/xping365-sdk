using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Net.Http.Headers;
using Microsoft.Playwright;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;

using Cookie = System.Net.Cookie;

namespace XPing365.Sdk.UnitTests.Components;

public sealed class TestSettignsTests
{
    [Test]
    public void PropertyBagIsNotNullWhenNewlyInstantiated()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Assert
        Assert.That(testSettings.PropertyBag, Is.Not.Null);
    }

    [Test]
    public void GetHttpMethodReturnsHttpGetWhenNotSpecified()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Assert
        Assert.That(testSettings.GetHttpMethod(), Is.EqualTo(HttpMethod.Get));
    }

    [Test]
    public void GetHttpMethodReturnsSpecifiedValueWhenSet()
    {
        // Arrange
        HttpMethod specifiedHttpMethod = HttpMethod.Post;
        var testSettings = new TestSettings();

        // Act
        testSettings.PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpMethod, specifiedHttpMethod);

        // Assert
        Assert.That(testSettings.GetHttpMethod(), Is.EqualTo(specifiedHttpMethod));
    }


    [Test]
    public void GetHttpMethodShouldReturnGetByDefault()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Act
        HttpMethod httpMethod = testSettings.GetHttpMethod();

        // Assert
        Assert.That(httpMethod, Is.EqualTo(HttpMethod.Get));
    }

    [Test]
    public void SetHttpContentStoresHttpContentWhenConfiured()
    {
        // Arrange
        var testSettings = new TestSettings();
        using HttpContent httpContent = new StringContent("http content");

        // Act
        testSettings.SetHttpContent(httpContent);

        // Assert
        Assert.That(testSettings.GetHttpContent(), Is.EqualTo(httpContent));
    }

    [Test]
    public void SetHttpContentShouldThowExceptionWhenNullProved()
    {
        // Arrange
        var testSettings = new TestSettings();
        HttpContent httpContent = null!;

        // Assert
        Assert.Throws<ArgumentNullException>(() => testSettings.SetHttpContent(httpContent));
    }

    [Test]
    public void SetHttpContentShouldSetContentTypeWhenStringContent()
    {
        // Arrange
        var testSettings = new TestSettings();
        using HttpContent httpContent = new StringContent("http content");

        // Act
        testSettings.SetHttpContent(httpContent);

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.ContentType, out var headers), Is.True);
        Assert.That(headers?.First(), Is.Not.Null);
        Assert.That(headers?.First(), Is.EqualTo("text/plain"));
    }

    [Test]
    public void SetHttpContentShouldSetContentTypeWhenJsonContent()
    {
        // Arrange
        var testSettings = new TestSettings();
        using HttpContent httpContent = JsonContent.Create("{\"name\":\"John\", \"age\":30, \"car\":null}");

        // Act
        testSettings.SetHttpContent(httpContent);

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.ContentType, out var headers), Is.True);
        Assert.That(headers?.First(), Is.Not.Null);
        Assert.That(headers?.First(), Is.EqualTo("application/json"));
    }

    [Test]
    public void SetHttpContentShouldSetContentTypeWhenFormUrlEncodedContent()
    {
        // Arrange
        var testSettings = new TestSettings();
        using HttpContent httpContent = new FormUrlEncodedContent([new KeyValuePair<string, string>("", "")]);

        // Act
        testSettings.SetHttpContent(httpContent);

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.ContentType, out var headers), Is.True);
        Assert.That(headers?.First(), Is.Not.Null);
        Assert.That(headers?.First(), Is.EqualTo("application/x-www-form-urlencoded"));
    }

    [Test]
    public void SetHttpContentShouldSetContentTypeWhenMultipartFormDataContent()
    {
        // Arrange
        var testSettings = new TestSettings();
        using HttpContent httpContent = new MultipartFormDataContent();

        // Act
        testSettings.SetHttpContent(httpContent);

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.ContentType, out var headers), Is.True);
        Assert.That(headers?.First(), Is.Not.Null);
        Assert.That(headers?.First(), Is.EqualTo("multipart/form-data"));
    }

    [Test]
    public void SetHttpContentShouldSetContentTypeWhenByteArrayContent()
    {
        // Arrange
        var testSettings = new TestSettings();
        using HttpContent httpContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Text"));

        // Act
        testSettings.SetHttpContent(httpContent);

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.ContentType, out var headers), Is.True);
        Assert.That(headers?.First(), Is.Not.Null);
        Assert.That(headers?.First(), Is.EqualTo("application/octet-stream"));
    }

    [Test]
    public void SetHttpContentShouldSetContentTypeWhenStreamContent()
    {
        // Arrange
        var testSettings = new TestSettings();
        using var memoryStream = new MemoryStream();
        using HttpContent httpContent = new StreamContent(memoryStream);

        // Act
        testSettings.SetHttpContent(httpContent);

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.ContentType, out var headers), Is.True);
        Assert.That(headers?.First(), Is.Not.Null);
        Assert.That(headers?.First(), Is.EqualTo("application/octet-stream"));
    }

    [Test]
    public void SetHttpContentShouldOverrideContentTypeWhenContentTypeAlreadyPresented()
    {
        // Arrange
        var testSettings = new TestSettings();
        using var memoryStream = new MemoryStream();
        using HttpContent httpContent = new StreamContent(memoryStream);
        testSettings.SetHttpRequestHeaders(new Dictionary<string, IEnumerable<string>>
        {
            { HeaderNames.ContentType, ["CustomContentType"] }
        });

        // Act
        testSettings.SetHttpContent(httpContent);

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.ContentType, out var headers), Is.True);
        Assert.That(headers?.First(), Is.Not.Null);
        Assert.That(headers.Count(), Is.EqualTo(1));
        Assert.That(headers?.First(), Is.EqualTo("application/octet-stream"));
    }

    [Test]
    public void SetHttpContentShouldNotOverrideContentTypeWhenContentTypeAlreadyPresentedAndSetHeadersDisabled()
    {
        // Arrange
        var testSettings = new TestSettings();
        using var memoryStream = new MemoryStream();
        using HttpContent httpContent = new StreamContent(memoryStream);
        testSettings.SetHttpRequestHeaders(new Dictionary<string, IEnumerable<string>>
        {
            { HeaderNames.ContentType, ["CustomContentType"] }
        });

        // Act
        testSettings.SetHttpContent(httpContent, setContentHeaders: false);

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.ContentType, out var headers), Is.True);
        Assert.That(headers?.First(), Is.Not.Null);
        Assert.That(headers.Count(), Is.EqualTo(1));
        Assert.That(headers?.First(), Is.EqualTo("CustomContentType"));
    }

    class CustomContentType : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    [Test]
    public void SetHttpContentShouldNotSetContentTypeWhenCustomContentType()
    {
        // Arrange
        var testSettings = new TestSettings();
        using HttpContent httpContent = new CustomContentType();

        // Act
        testSettings.SetHttpContent(httpContent);

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.ContentType, out var headers), Is.False);
        Assert.That(headers, Is.Null);
    }

    [Test]
    public void GetHttpContentShouldReturnNullWhenNotSpecified()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Assert
        Assert.That(testSettings.GetHttpContent(), Is.Null);
    }

    [Test]
    public void GetHttpContentReturnsSpecifiedValueWhenSet()
    {
        // Arrange
        var testSettings = new TestSettings();
        using HttpContent httpContent = new StringContent("http content");

        // Act
        testSettings.SetHttpContent(httpContent);

        // Assert
        Assert.That(testSettings.GetHttpContent(), Is.EqualTo(httpContent));
    }

    [Test]
    public void ClearHttpContentRemovesHttpContentFromTestSettings()
    {
        // Arrange
        var testSettings = new TestSettings();
        using HttpContent httpContent = new StringContent("http content");
        testSettings.SetHttpContent(httpContent);

        // Act
        testSettings.ClearHttpContent();

        // Assert
        Assert.That(testSettings.GetHttpContent(), Is.Null);
    }

    [Test]
    public void ClearHttpContentDoesNothingWhenHttpContentNotSpecifiedPreviously()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Assert
        Assert.DoesNotThrow(() => testSettings.ClearHttpContent());
    }

    [Test]
    public void SetHttpRequestHeadersShouldSetsHttpHeadersWhenSpecified()
    {
        // Arrange
        var testSettings = new TestSettings();
        var httpRequestHeaders = new Dictionary<string, IEnumerable<string>>
        {
            { HeaderNames.UserAgent, ["Chrome/51.0.2704.64 Safari/537.36"] }
        };

        // Act
        testSettings.SetHttpRequestHeaders(httpRequestHeaders);

        // Assert
        Assert.That(testSettings.PropertyBag.ContainsKey(PropertyBagKeys.HttpRequestHeaders), Is.True);
    }

    [Test]
    public void ClearHttpRequestHeadersShouldClearsHttpHeaders()
    {
        // Arrange
        var httpRequestHeaders = new Dictionary<string, IEnumerable<string>>
        {
            { HeaderNames.UserAgent, ["Chrome/51.0.2704.64 Safari/537.36"] }
        };
        var testSettings = TestSettings.Default;
        testSettings.SetHttpRequestHeaders(httpRequestHeaders);

        // Act
        testSettings.ClearHttpRequestHeaders();

        // Assert
        Assert.That(testSettings.PropertyBag.ContainsKey(PropertyBagKeys.HttpRequestHeaders), Is.False);
    }

    [Test]
    public void ClearHttpRequestHeadersShouldDoNothingWhenHttpRequestHeadersWereNotSetPreviously()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Act
        testSettings.ClearHttpRequestHeaders();

        // Assert
        Assert.That(testSettings.PropertyBag.ContainsKey(PropertyBagKeys.HttpRequestHeaders), Is.False);
    }

    [Test]
    public void GetHttpRequestHeadersShouldReturnEmptyDictionaryWhenNotSpecified()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Assert
        Assert.That(testSettings.GetHttpRequestHeaders(), Is.Empty);
    }

    [Test]
    public void GetHttpRequestHeadersShouldReturnSpecifiedDictionaryWhenSet()
    {
        // Arrange
        var testSettings = new TestSettings();
        var httpRequestHeaders = new Dictionary<string, IEnumerable<string>>
        {
            { HeaderNames.UserAgent, ["Chrome/51.0.2704.64 Safari/537.36"] }
        };

        // Act
        testSettings.PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpRequestHeaders, httpRequestHeaders);

        // Assert
        Assert.That(testSettings.GetHttpRequestHeaders(), Is.EqualTo(httpRequestHeaders));
    }

    [Test]
    public void SetHttpMethodShouldStoreHttpMethod()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Act
        testSettings.SetHttpMethod(HttpMethod.Put);

        // Assert
        Assert.That(testSettings.GetHttpMethod(), Is.EqualTo(HttpMethod.Put));
    }

    [Test]
    public void DefaultTestSettingsAlwaysReturnNewInstanceWhenCalled()
    {
        // To not affect default instance parameters, this instance should always be recreated.
        var testSettings1 = TestSettings.Default;
        var testSettings2 = TestSettings.Default;

        Assert.That(testSettings1, Is.Not.EqualTo(testSettings2));
    }

    [Test]
    public void AddCookieStoresCookie()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Act
        testSettings.AddCookie(new Cookie("cookiename", "value"));

        // Assert
        Assert.That(testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.Cookie, out var cookies), Is.True);
        Assert.That(cookies, Is.Not.Null);
        Assert.That(cookies.First(), Is.EqualTo("cookiename=value"));
    }

    [Test]
    public void AddMultipleCookiesStoresMultipleCookies()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Act
        testSettings.AddCookie(new Cookie("cookiename1", "value1"));
        testSettings.AddCookie(new Cookie("cookiename2", "value2"));
        testSettings.AddCookie(new Cookie("cookiename3", "value3"));

        // Assert
        Assert.That(testSettings.GetCookies(), Has.Count.EqualTo(3));
    }

    [Test]
    public void GetCookiesReturnStoredCookies()
    {
        // Arrange
        var testSettings = new TestSettings();
        var cookie = new Cookie("cookiename", "value");

        // Act
        testSettings.AddCookie(cookie);

        // Assert
        Assert.That(testSettings.GetCookies(), Has.Count.EqualTo(1));
        Assert.That(testSettings.GetCookies().First(), Is.EqualTo(cookie));
    }

    [Test]
    public void ClearCookiesClearsStoredCookies()
    {
        // Arrange
        var testSettings = new TestSettings();
        var cookie = new Cookie("cookiename", "value");
        testSettings.AddCookie(cookie);

        // Act
        testSettings.ClearCookies();

        // Assert
        Assert.That(testSettings.GetCookies(), Has.Count.EqualTo(0));
    }

    [Test]
    public void AddCookieCollectionStoresCookies()
    {
        // Arrange
        var testSettings = new TestSettings();
        var cookies = new CookieCollection
        {
            new Cookie("cookiename1", "value1"),
            new Cookie("cookiename2", "value2")
        };

        // Act
        testSettings.AddCookies(cookies);

        // Assert
        Assert.That(testSettings.GetCookies(), Has.Count.EqualTo(2));
    }

    [Test]
    public void SetCredentialsShouldStoreCredentials()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Act
        testSettings.SetCredentials(new NetworkCredential("userName", "password"));

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.Authorization, out var headers), Is.True);
        Assert.That(headers?.First(), Is.Not.Null);
        Assert.That(headers?.First(), Is.EqualTo("Basic dXNlck5hbWU6cGFzc3dvcmQ="));
    }

    [Test]
    public void ClearCredentialsShouldClearCredentials()
    {
        // Arrange
        var testSettings = new TestSettings();
        testSettings.SetCredentials(new NetworkCredential("userName", "password"));

        // Act
        testSettings.ClearCredentials();

        // Assert
        Assert.That(
            testSettings.GetHttpRequestHeaders().TryGetValue(HeaderNames.Authorization, out var headers), Is.False);
        Assert.That(headers?.First(), Is.Null);
    }

    [Test]
    public void SetGeolocationStoresCoordinates()
    {
        // Arrange
        var geolocation = new Geolocation()
        { 
            Latitude = 45, 
            Longitude = 90, 
            Accuracy = 100 
        };
        var testSettings = new TestSettings();

        // Act
        testSettings.SetGeolocation(geolocation);

        // Assert
        Assert.That(
            testSettings.PropertyBag.GetProperty<Geolocation>(PropertyBagKeys.Geolocation), 
            Is.EqualTo(geolocation));
    }

    [Test]
    public void GetGeolocationRetrievesCoordinates()
    {
        // Arrange
        var geolocation = new Geolocation()
        {
            Latitude = 45,
            Longitude = 90,
            Accuracy = 100
        };
        var testSettings = new TestSettings();
        testSettings.SetGeolocation(geolocation);

        // Act
        var coordinates = testSettings.GetGeolocation();

        // Assert
        Assert.That(coordinates, Is.EqualTo(geolocation));
    }

    [Test]
    public void ClearGeolocationRemovesCoordinates()
    {
        // Arrange
        var geolocation = new Geolocation()
        {
            Latitude = 45,
            Longitude = 90,
            Accuracy = 100
        };
        var testSettings = new TestSettings();
        testSettings.SetGeolocation(geolocation);

        // Act
        testSettings.ClearGeolocation();

        // Assert
        Assert.That(testSettings.GetGeolocation(), Is.Null);
    }
}
