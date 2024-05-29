using System.Globalization;
using System.Reflection.Metadata;
using HtmlAgilityPack;
using Moq;
using XPing365.Sdk.Availability.UnitTests.Validations.Content.Html.Helpers;
using XPing365.Sdk.Availability.Validations;
using XPing365.Sdk.Availability.Validations.Content.Html;
using XPing365.Sdk.Availability.Validations.Content.Html.Internals;
using XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Session;
using TestContext = XPing365.Sdk.Core.Components.TestContext;

namespace XPing365.Sdk.Availability.UnitTests.Validations.Content.Html.Internals;

public sealed class InstrumentedHtmlContentTests
{
    private readonly TestContext _testContext = HtmlContentTestsHelpers.CreateTestContext();

    private sealed class InstrumentedHtmlContentUnderTest(string data, TestContext context, string testIdAttribute) : 
        InstrumentedHtmlContent(data, context, testIdAttribute)
    {
        private readonly Mock<ISelector> _mockedSelector = new();

        public Mock<ISelector> MockedSelector => _mockedSelector;

        protected override ISelector CreateByTitleSelector()
        {
            return _mockedSelector.Object;
        }
    }

    [Test]
    public void CtorThrowsArgumentExceptionWhenDataIsNull()
    {
        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            new InstrumentedHtmlContent(data: null!, _testContext, "testIdAttribute");
        });
    }

    [Test]
    public void CtorThrowsArgumentExceptionWhenDataIsEmpty()
    {
        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            new InstrumentedHtmlContent(data: string.Empty, _testContext, "testIdAttribute");
        });
    }

    [Test]
    public void CtorThrowsArgumentNullExceptionWhenTestContextIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            new InstrumentedHtmlContent(data: "<html></html>", context: null!, "testIdAttribute");
        });
    }

    [Test]
    public void CtorThrowsArgumentExceptionWhenTestIdAttributeIsNull()
    {
        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            new InstrumentedHtmlContent(data: "<html></html>", _testContext, testIdAttribute: null!);
        });
    }

    [Test]
    public void CtorThrowsArgumentExceptionWhenTestIdAttributeIsEmpty()
    {
        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            new InstrumentedHtmlContent(data: "<html></html>", _testContext, testIdAttribute: string.Empty);
        });
    }

    [Test]
    public void HasTitleBuildsTestSession()
    {
        // Arrange
        var expectedTitle = "Title";
        var instrumentedHtml = new InstrumentedHtmlContent(
            HtmlContentTestsHelpers.GetTitleHtml(title: expectedTitle),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var options = new TextOptions() { Exact = false };

        // Act
        instrumentedHtml.HasTitle(expectedTitle, options);

        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(instrumentedHtml.HasTitle))))), Times.Once);
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("title")),
                It.Is<IPropertyBagValue>(_p => 
                    _p.Equals(new PropertyBagValue<string>(expectedTitle)))), Times.Once);
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey(nameof(TextOptions))),
                It.Is<IPropertyBagValue>(_p => 
                    _p.Equals(new PropertyBagValue<string>(options.ToString())))), Times.Once);
    }

    [Test]
    public void HasTitleBuildsTestSessionWhenProvidedTextOptionsIsNull()
    {
        // Arrange
        var expectedTitle = "Title";
        var instrumentedHtml = new InstrumentedHtmlContent(
            HtmlContentTestsHelpers.GetTitleHtml(title: expectedTitle),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");

        // Act
        instrumentedHtml.HasTitle(expectedTitle, options: null);

        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey(nameof(TextOptions))),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>("Null")))), Times.Once);
    }

    [Test]
    public void HasTitleBuildsTestSessionWithSelectorResults()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetTitleHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var htmlNodeCollection = new HtmlNodeCollection(
            parentnode: new HtmlNode(HtmlNodeType.Element, instrumentedHtml.Document, 0))
        {
            HtmlNode.CreateNode("<title>expectedTitle</title>"),
        };

        instrumentedHtml.MockedSelector
            .Setup(_m => _m.Select(It.IsAny<HtmlNode>()))
            .Returns(htmlNodeCollection);

        // Act
        instrumentedHtml.HasTitle("expectedTitle");

        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("Nodes")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string[]>(
                        htmlNodeCollection.Select(n => n.OriginalName.Trim()).ToArray())))), Times.Once);
    }

    [Test]
    public void HasTitleShouldCreateSuccessfulTestStepWhenCompletedSuccessfully()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetTitleHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var htmlNodeCollection = new HtmlNodeCollection(
            parentnode: new HtmlNode(HtmlNodeType.Element, instrumentedHtml.Document, 0))
        {
            HtmlNode.CreateNode("<title>expectedTitle</title>"),
        };

        instrumentedHtml.MockedSelector
            .Setup(_m => _m.Select(It.IsAny<HtmlNode>()))
            .Returns(htmlNodeCollection);

        // Act
        instrumentedHtml.HasTitle("expectedTitle");

        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder).Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void HasTitleInvokesReportOnProgressContextWhenProgressInstanceAvailable()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetTitleHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var htmlNodeCollection = new HtmlNodeCollection(
            parentnode: new HtmlNode(HtmlNodeType.Element, instrumentedHtml.Document, 0))
        {
            HtmlNode.CreateNode("<title>expectedTitle</title>"),
        };

        instrumentedHtml.MockedSelector
            .Setup(_m => _m.Select(It.IsAny<HtmlNode>()))
            .Returns(htmlNodeCollection);

        // Act
        instrumentedHtml.HasTitle("expectedTitle");

        // Assert
        Mock.Get(instrumentedHtml.Context.Progress!).Verify(_m => _m.Report(It.IsAny<TestStep>()), Times.Once);
    }

    [Test]
    public void HasTitleThrowsValidationExceptionWhenSelectorReturnsNoNodes()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetTitleHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var htmlNodeCollection = new HtmlNodeCollection(
            parentnode: new HtmlNode(HtmlNodeType.Element, instrumentedHtml.Document, 0))
        {
            // No HtmlNode instance created simulating a collection with zero nodes.
        };

        instrumentedHtml.MockedSelector
            .Setup(_m => _m.Select(It.IsAny<HtmlNode>()))
            .Returns(htmlNodeCollection);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => instrumentedHtml.HasTitle("expectedTitle"));

        Assert.That(exception.Message, Is.EqualTo(
            "No <title> node available. Ensure that the html content has <title> node before attempting " +
            "to validate its value. This exception occurred as part of validating HTML data."));
    }

    [Test]
    public void HasTitleThrowsValidationExceptionWhenSelectorReturnsMoreThanOneNode()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetTitleHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var htmlNodeCollection = new HtmlNodeCollection(
            parentnode: new HtmlNode(HtmlNodeType.Element, instrumentedHtml.Document, 0))
        {
            HtmlNode.CreateNode("<title>expectedTitle</title>"),
            HtmlNode.CreateNode("<title>expectedTitle</title>"),
        };

        instrumentedHtml.MockedSelector
            .Setup(_m => _m.Select(It.IsAny<HtmlNode>()))
            .Returns(htmlNodeCollection);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => instrumentedHtml.HasTitle("expectedTitle"));

        Assert.That(exception.Message, Is.EqualTo(
            "Multiple <title> nodes were found. The method expects a single <title> node under the <head> " +
            "node. Please ensure that the HTML content contains only one <title> node for proper validation. " +
            "This exception occurred as part of validating HTML data."));
    }

    [Test]
    public void HasTitleThrowsValidationExceptionWhenTitlesDoesNotMatch()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetTitleHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var htmlNodeCollection = new HtmlNodeCollection(
            parentnode: new HtmlNode(HtmlNodeType.Element, instrumentedHtml.Document, 0))
        {
            HtmlNode.CreateNode("<title>expectedTitle</title>"),
        };

        instrumentedHtml.MockedSelector
            .Setup(_m => _m.Select(It.IsAny<HtmlNode>()))
            .Returns(htmlNodeCollection);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => instrumentedHtml.HasTitle("OtherTitle"));

        Assert.That(exception.Message, Is.EqualTo(
            $"Expected the <title> node's inner text to be \"OtherTitle\", but the actual <title> node's inner " +
            $"text was \"expectedTitle\". This exception occurred as part of validating HTML data."));
    }

    [Test]
    public void HasMaxDocumentSizeBuildsTestSession()
    {
        // Arrange
        int maxSizeInBytes = 150;
        var instrumentedHtml = new InstrumentedHtmlContent(
            HtmlContentTestsHelpers.GetTitleHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");

        // Act
        instrumentedHtml.HasMaxDocumentSize(maxSizeInBytes);

        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(instrumentedHtml.HasMaxDocumentSize))))), Times.Once);
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("maxSizeInBytes")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(maxSizeInBytes.ToString(CultureInfo.InvariantCulture))))),
                        Times.Once);
    }

    [Test]
    public void HasMaxDocumentSizeBuildsTestSessionWithActualResults()
    {
        // Arrange
        var html = HtmlContentTestsHelpers.GetTitleHtml();
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            html, HtmlContentTestsHelpers.CreateTestContext(), "data-testid");

        // Act
        instrumentedHtml.HasMaxDocumentSize(maxSizeInBytes: 150);
        var document = instrumentedHtml.Document;
        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("SizeInBytes")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(
                        document.Encoding.GetByteCount(document.Text).ToString(CultureInfo.InvariantCulture))))), 
                        Times.Once);
    }

    [Test]
    public void HasMaxDocumentSizeShouldCreateSuccessfulTestStepWhenCompletedSuccessfully()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetTitleHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");

        // Act
        instrumentedHtml.HasMaxDocumentSize(maxSizeInBytes: 150);

        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder).Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void HasMaxDocumentSizeInvokesReportOnProgressContextWhenProgressInstanceAvailable()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetTitleHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");

        // Act
        instrumentedHtml.HasMaxDocumentSize(maxSizeInBytes: 150);

        // Assert
        Mock.Get(instrumentedHtml.Context.Progress!).Verify(_m => _m.Report(It.IsAny<TestStep>()), Times.Once);
    }

    [Test]
    public void HasMaxDocumentSizeThrowsValidationExceptionWhenActualSizeIsGreaterThanMaxSizeAllowed()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetTitleHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() =>
        {
            instrumentedHtml.HasMaxDocumentSize(maxSizeInBytes: 5);
        });

        var document = instrumentedHtml.Document;
        var byteCount = document.Encoding.GetByteCount(document.Text).ToString(CultureInfo.InvariantCulture);

        Assert.That(exception.Message, Is.EqualTo(
            $"The expected HTML document size should be equal to or less than 5 bytes; however, " +
            $"the actual size was {byteCount} bytes. This exception occurred as part of validating HTML data."));
    }

    [Test]
    public void HasMaxDocumentSizeOperatesOnEmptyHtmlDocument()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            data: string.Empty,
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");

        // Act & Assert
        Assert.DoesNotThrow(() => instrumentedHtml.HasMaxDocumentSize(maxSizeInBytes: 5));
    }

    [Test]
    public void GetByAltTextBuildsTestSession()
    {
        // Arrange
        var altText = "logo";
        var instrumentedHtml = new InstrumentedHtmlContent(
            HtmlContentTestsHelpers.GetAltHtml(altText),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var options = new TextOptions() { Exact = false };

        // Act
        instrumentedHtml.GetByAltText(text: altText, options);

        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(instrumentedHtml.GetByAltText))))), Times.Once);
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("text")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(altText)))), Times.Once);
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey(nameof(TextOptions))),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(options.ToString())))), Times.Once);
    }

    [Test]
    public void GetByAltTextBuildsTestSessionWhenProvidedTextOptionsIsNull()
    {
        // Arrange
        var altText = "logo";
        var instrumentedHtml = new InstrumentedHtmlContent(
            HtmlContentTestsHelpers.GetAltHtml(altText),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");

        // Act
        instrumentedHtml.GetByAltText(text: altText, options: null);

        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey(nameof(TextOptions))),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>("Null")))), Times.Once);
    }

    [Test]
    public void GetByAltTextBuildsTestSessionWithSelectorResults()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetAltHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var htmlNodeCollection = new HtmlNodeCollection(
            parentnode: new HtmlNode(HtmlNodeType.Element, instrumentedHtml.Document, 0))
        {
            HtmlNode.CreateNode($"<img alt=\"logo\" src=\"/img/xping365-logo.svg\" width=\"100\" />"),
        };

        instrumentedHtml.MockedSelector
            .Setup(_m => _m.Select(It.IsAny<HtmlNode>()))
            .Returns(htmlNodeCollection);

        // Act
        instrumentedHtml.GetByAltText(text: "logo");

        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("Nodes")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string[]>(
                        htmlNodeCollection.Select(n => n.OriginalName.Trim()).ToArray())))), Times.Once);
    }

    [Test]
    public void GetByAltTextShouldCreateSuccessfulTestStepWhenCompletedSuccessfully()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetAltHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var htmlNodeCollection = new HtmlNodeCollection(
            parentnode: new HtmlNode(HtmlNodeType.Element, instrumentedHtml.Document, 0))
        {
            HtmlNode.CreateNode($"<img alt=\"logo\" src=\"/img/xping365-logo.svg\" width=\"100\" />"),
        };

        instrumentedHtml.MockedSelector
            .Setup(_m => _m.Select(It.IsAny<HtmlNode>()))
            .Returns(htmlNodeCollection);

        // Act
        instrumentedHtml.GetByAltText(text: "logo");

        // Assert
        Mock.Get(instrumentedHtml.Context.SessionBuilder).Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void GetByAltTextInvokesReportOnProgressContextWhenProgressInstanceAvailable()
    {
        // Arrange
        var instrumentedHtml = new InstrumentedHtmlContentUnderTest(
            HtmlContentTestsHelpers.GetAltHtml(),
            HtmlContentTestsHelpers.CreateTestContext(),
            "data-testid");
        var htmlNodeCollection = new HtmlNodeCollection(
            parentnode: new HtmlNode(HtmlNodeType.Element, instrumentedHtml.Document, 0))
        {
            HtmlNode.CreateNode($"<img alt=\"logo\" src=\"/img/xping365-logo.svg\" width=\"100\" />"),
        };

        instrumentedHtml.MockedSelector
            .Setup(_m => _m.Select(It.IsAny<HtmlNode>()))
            .Returns(htmlNodeCollection);

        // Act
        instrumentedHtml.GetByAltText(text: "logo");

        // Assert
        Mock.Get(instrumentedHtml.Context.Progress!).Verify(_m => _m.Report(It.IsAny<TestStep>()), Times.Once);
    }
}
