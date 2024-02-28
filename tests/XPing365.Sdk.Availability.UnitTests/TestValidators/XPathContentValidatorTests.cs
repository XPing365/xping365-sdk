using System.Xml.XPath;
using Moq;
using XPing365.Sdk.Availability.TestValidators;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;
using TestContext = XPing365.Sdk.Core.Components.TestContext;

namespace XPing365.Sdk.Availability.UnitTests.TestValidators;

public sealed class XPathContentValidatorTests
{
    [Test]
    public void CtorThrowsArgumentNullExceptionWhenNullXPathIsGiven()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new XPathContentValidator(xpath: null!, nodes => true));
    }

    [Test]
    public void CtorThrowsArgumentNullExceptionWhenNullValidationFunctionIsGiven()
    {
        // Arrange
        XPathExpression xpath = XPathExpression.Compile("//div");

        // Assert
        Assert.Throws<ArgumentNullException>(() => new XPathContentValidator(xpath, isValid: null!));
    }

    [Test]
    public void HandleAsyncThrowsArgumentNullExceptionWhenUrlIsNull()
    {
        // Arrange
        XPathExpression xpath = XPathExpression.Compile("//div");
        var validator = new XPathContentValidator(xpath, nodes => true);
        var context = new TestContext(Mock.Of<ITestSessionBuilder>(), progress: null);

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
            validator.HandleAsync(url: null!, TestSettings.DefaultForHttpClient, context, Mock.Of<IServiceProvider>()));
    }

    [Test]
    public void HandleAsyncThrowsArgumentNullExceptionWhenSettingsIsNull()
    {
        // Arrange
        var url = new Uri("http://localhost");
        XPathExpression xpath = XPathExpression.Compile("//div");
        var validator = new XPathContentValidator(xpath, nodes => true);
        var context = new TestContext(Mock.Of<ITestSessionBuilder>(), progress: null);

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
            validator.HandleAsync(url, settings: null!, context, Mock.Of<IServiceProvider>()));
    }

    [Test]
    public void HandleAsyncThrowsArgumentNullExceptionWhenContextIsNull()
    {
        // Arrange
        var url = new Uri("http://localhost");
        XPathExpression xpath = XPathExpression.Compile("//div");
        var validator = new XPathContentValidator(xpath, nodes => true);
        TestSettings settings = TestSettings.DefaultForHttpClient;

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
            validator.HandleAsync(url, settings, context: null!, Mock.Of<IServiceProvider>()));
    }

    [Test]
    public void HandleAsyncDoesNotThrowExceptionWhenHttpResponseIsNotXmlBasedType()
    {
        // Arrange
        var url = new Uri("http://localhost");
        TestSettings settings = TestSettings.DefaultForHttpClient;
        var context = new TestContext(new TestSessionBuilder(), progress: null);
        using var instrumentation = new InstrumentationLog();

        using var response = new HttpResponseMessage
        {
            Content = new StringContent("This is not XML based document") 
        };

        byte[] data = response.Content.ReadAsByteArrayAsync().Result;

        var componentMock = new Mock<ITestComponent>();
        componentMock.SetupGet(c => c.Name).Returns(nameof(componentMock));
        componentMock.SetupGet(c => c.Type).Returns(TestStepType.ValidateStep);

        XPathExpression xpath = XPathExpression.Compile("//div");
        var validator = new XPathContentValidator(xpath, nodes => nodes.Count == 0);

        // Act
        context.SessionBuilder
            .Build(PropertyBagKeys.HttpResponseMessage, new NonSerializable<HttpResponseMessage>(response))
            .Build(PropertyBagKeys.HttpContent, new PropertyBagValue<byte[]>(data))
            .Build(component: componentMock.Object, instrumentation);

        // Assert
        Assert.DoesNotThrow(() => validator.HandleAsync(url, settings, context, Mock.Of<IServiceProvider>()));
    }

    [Test]
    public void HandleAsyncCorrectlyResolveXPathWhenHtmlContentProvided()
    {
        // Arrange
        var url = new Uri("http://localhost");
        TestSettings settings = TestSettings.DefaultForHttpClient;
        var context = new TestContext(new TestSessionBuilder(), progress: null);
        using var instrumentation = new InstrumentationLog();

        using var response = new HttpResponseMessage
        {
            Content = new StringContent("<html><head></head><body><div></div></body></html>")
        };

        byte[] data = response.Content.ReadAsByteArrayAsync().Result;

        var componentMock = new Mock<ITestComponent>();
        componentMock.SetupGet(c => c.Name).Returns(nameof(componentMock));
        componentMock.SetupGet(c => c.Type).Returns(TestStepType.ValidateStep);

        XPathExpression xpath = XPathExpression.Compile("//div");
        var validator = new XPathContentValidator(xpath, nodes => nodes.Count == 1);

        // Act
        context.SessionBuilder
            .Build(PropertyBagKeys.HttpResponseMessage, new NonSerializable<HttpResponseMessage>(response))
            .Build(PropertyBagKeys.HttpContent, new PropertyBagValue<byte[]>(data))
            .Build(component: componentMock.Object, instrumentation);

        // Assert
        Assert.DoesNotThrow(() => validator.HandleAsync(url, settings, context, Mock.Of<IServiceProvider>()));
    }
}
