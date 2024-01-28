using Microsoft.Net.Http.Headers;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.UnitTests.Core;

public sealed class TestSettignsTests
{
    [Test]
    public void PropertyBagIsNotNullWhenNewlyinstantiated()
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
    public void GetHttpContentOrDefaultReturnsNullWhenNotSpecified()
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
        testSettings.PropertyBag.AddOrUpdateProperty(PropertyBagKeys.HttpContent, httpContent);

        // Assert
        Assert.That(testSettings.GetHttpContent(), Is.EqualTo(httpContent));
    }

    [Test]
    public void GetHttpRequestHeadersOrEmptyReturnsEmptyDictionaryWhenNotSpecified()
    {
        // Arrange
        var testSettings = new TestSettings();

        // Assert
        Assert.That(testSettings.GetHttpRequestHeadersOrEmpty(), Is.Empty);
    }

    [Test]
    public void GetHttpRequestHeadersOrEmptyReturnsSpecifiedDictionaryWhenSet()
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
        Assert.That(testSettings.GetHttpRequestHeadersOrEmpty(), Is.EqualTo(httpRequestHeaders));
    }
}
