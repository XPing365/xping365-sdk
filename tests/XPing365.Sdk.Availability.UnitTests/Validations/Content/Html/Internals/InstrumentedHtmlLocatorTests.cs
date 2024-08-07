using System.Xml.XPath;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Moq;
using XPing365.Sdk.Availability.UnitTests.Validations.Content.Html.Helpers;
using XPing365.Sdk.Availability.Validations;
using XPing365.Sdk.Availability.Validations.Content.Html;
using XPing365.Sdk.Availability.Validations.Content.Html.Internals;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Session;
using TestContext = XPing365.Sdk.Core.Components.TestContext;

namespace XPing365.Sdk.Availability.UnitTests.Validations.Content.Html.Internals;

public sealed class InstrumentedHtmlLocatorTests
{
    private readonly TestContext _testContext = HtmlContentTestsHelpers.CreateTestContext();
    private Mock<IIterator<HtmlNode>> _iteratorMock;

    [SetUp]
    public void SetUp()
    {
        _iteratorMock = new Mock<IIterator<HtmlNode>>();
    }

    [Test]
    public void CtorThrowsArgumentNullExceptionWhenNodesAreNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            new InstrumentedHtmlLocator(
                nodes: null!,
                iterator: _iteratorMock.Object,
                context: _testContext);
        });
    }

    [Test]
    public void CtorThrowsArgumentNullExceptionWhenIteratorIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            new InstrumentedHtmlLocator(
                nodes: HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 0),
                iterator: null!,
                context: _testContext);
        });
    }

    [Test]
    public void CtorThrowsArgumentNullExceptionWhenContextIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            new InstrumentedHtmlLocator(
                nodes: HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 0),
                iterator: _iteratorMock.Object,
                context: null!);
        });
    }

    [Test]
    public void CtorDoesNotAdvanceIteratorWhenNodesAreEmpty()
    {
        // Arrange & Act
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 0),
            iterator: _iteratorMock.Object,
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Assert
        _iteratorMock.Verify(i => i.First(), Times.Never);
        _iteratorMock.Verify(i => i.Last(), Times.Never);
        _iteratorMock.Verify(i => i.Nth(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void CtorAdvancesIteratorByCallingFirstWhenNodesAreNotEmpty()
    {
        // Arrange & Act
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3),
            iterator: _iteratorMock.Object,
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Assert
        _iteratorMock.Verify(i => i.First(), Times.Once);
        _iteratorMock.Verify(i => i.Last(), Times.Never);
        _iteratorMock.Verify(i => i.Nth(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void FirstBuildsTestSession()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act
        htmlLocator.First();

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.First))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("Nodes")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray())))),
                        Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nodes.First().OriginalName.Trim())))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void FirstAdvancesToFirstItem()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var iterator = new HtmlNodeIterator(nodes);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: iterator,
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act
        htmlLocator.First();

        // Assert
        Assert.That(iterator.IsAdvanced, Is.True);
        Assert.That(iterator.Current(), Is.Not.Null);
        Mock.Get(htmlLocator.Context.SessionBuilder)
           .Verify(_m => _m.Build(
               It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
               It.Is<IPropertyBagValue>(_p =>
                   _p.Equals(new PropertyBagValue<string>(nodes.First().OriginalName.Trim())))), Times.Once);
    }

    [Test]
    public void FirstDoesNotAdvanceToFirstItemWhenNodesEmpty()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 0);
        var iterator = new HtmlNodeIterator(nodes);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: iterator,
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act
        htmlLocator.First();

        // Assert
        Assert.That(iterator.IsAdvanced, Is.False);
        Assert.That(iterator.Current(), Is.Null);
        Mock.Get(htmlLocator.Context.SessionBuilder)
           .Verify(_m => _m.Build(
               It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
               It.Is<IPropertyBagValue>(_p =>
                   _p.Equals(new PropertyBagValue<string>("Null")))), Times.Once);
    }

    [Test]
    public void FirstInvokesReportOnProgressContextWhenProgressInstanceAvailable()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: _iteratorMock.Object,
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act
        htmlLocator.First();

        // Assert
        Mock.Get(htmlLocator.Context.Progress!).Verify(_m => _m.Report(It.IsAny<TestStep>()), Times.Once);
    }

    [Test]
    public void LastBuildsTestSession()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act
        htmlLocator.Last();

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.Last))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("Nodes")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray())))),
                        Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nodes.Last().OriginalName.Trim())))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void LastAdvancesToLastItem()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: _iteratorMock.Object,
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act
        htmlLocator.Last();

        // Assert
        _iteratorMock.Verify(_m => _m.Last(), Times.Once);
    }

    [Test]
    public void LastDoesNotAdvanceToLastItemWhenNodesEmpty()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 0);
        var iterator = new HtmlNodeIterator(nodes);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: iterator,
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act
        htmlLocator.Last();

        // Assert
        Assert.That(iterator.IsAdvanced, Is.False);
        Assert.That(iterator.Current(), Is.Null);
        Mock.Get(htmlLocator.Context.SessionBuilder)
           .Verify(_m => _m.Build(
               It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
               It.Is<IPropertyBagValue>(_p =>
                   _p.Equals(new PropertyBagValue<string>("Null")))), Times.Once);
    }

    [Test]
    public void LastInvokesReportOnProgressContextWhenProgressInstanceAvailable()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: _iteratorMock.Object,
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act
        htmlLocator.Last();

        // Assert
        Mock.Get(htmlLocator.Context.Progress!).Verify(_m => _m.Report(It.IsAny<TestStep>()), Times.Once);
    }

    [Test]
    public void FilterBuildsTestSessionWithEmptyNodes()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 0);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new FilterOptions { HasText = "Password" };
        // Act
        htmlLocator.Filter(options);

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.Filter))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>("Null")))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("FilterOptions")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(options.ToString())))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("ChildNodes")),
                It.IsAny<IPropertyBagValue>()), Times.Never);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("FilteredNodes")),
                It.IsAny<IPropertyBagValue>()), Times.Never);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void FilterBuildsTestSessionWithNotEmptyNodes()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new FilterOptions { HasText = "Password" };
        htmlLocator.First();
        Mock.Get(htmlLocator.Context.SessionBuilder).Invocations.Clear();

        // Act
        htmlLocator.Filter(options);

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.Filter))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>("label")))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("FilterOptions")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(options.ToString())))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("ChildNodes")),
                It.IsAny<IPropertyBagValue>()), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("FilteredNodes")),
                It.IsAny<IPropertyBagValue>()), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void FilterReturnsNewObject()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new FilterOptions { HasText = "Password" };

        htmlLocator.First();

        // Act
        var newLocator = htmlLocator.Filter(options);

        // Assert
        Assert.That(newLocator, Is.Not.EqualTo(htmlLocator));
    }

    [Test]
    public void FilterReturnsItSelfWhenNoResults()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new FilterOptions { HasText = "NotFoundNode" };

        htmlLocator.First();

        // Act
        var newLocator = htmlLocator.Filter(options);

        // Assert
        Assert.That(newLocator, Is.EqualTo(htmlLocator));
    }

    [Test]
    public void FilterInvokesReportOnProgressContextWhenProgressInstanceAvailable()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new FilterOptions { HasText = "Password" };

        htmlLocator.First();
        Mock.Get(htmlLocator.Context.Progress!).Invocations.Clear();

        // Act
        var newLocator = htmlLocator.Filter(options);

        // Assert
        Mock.Get(htmlLocator.Context.Progress!).Verify(_m => _m.Report(It.IsAny<TestStep>()), Times.Once);
    }

    [Test]
    public void LocatorBuildsTestSessionWithEmptyNodes()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 0);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new FilterOptions { HasText = "Password" };
        // Act
        htmlLocator.Locator(XPathExpression.Compile("//label"), options);

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.Locator))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>("Null")))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("FilterOptions")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(options.ToString())))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("ChildNodes")),
                It.IsAny<IPropertyBagValue>()), Times.Never);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("FilteredNodes")),
                It.IsAny<IPropertyBagValue>()), Times.Never);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void LocatorBuildsTestSessionWithNotEmptyNodes()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new FilterOptions { HasText = "Password" };
        htmlLocator.First();
        Mock.Get(htmlLocator.Context.SessionBuilder).Invocations.Clear();

        // Act
        htmlLocator.Locator(XPathExpression.Compile("//label"), options);

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.Locator))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>("label")))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("selector")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>("//label")))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("FilterOptions")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(options.ToString())))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("ChildNodes")),
                It.IsAny<IPropertyBagValue>()), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("LocatedNodes")),
                It.IsAny<IPropertyBagValue>()), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void LocatorReturnsNewObject()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new FilterOptions { HasText = "Password" };

        htmlLocator.First();

        // Act
        var newLocator = htmlLocator.Locator(XPathExpression.Compile("//label"), options);

        // Assert
        Assert.That(newLocator, Is.Not.EqualTo(htmlLocator));
    }

    [Test]
    public void LocatorReturnsItSelfWhenNoResults()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new FilterOptions { HasText = "NotFoundNode" };

        htmlLocator.First();

        // Act
        var newLocator = htmlLocator.Locator(XPathExpression.Compile("//notfound"), options);

        // Assert
        Assert.That(newLocator, Is.EqualTo(htmlLocator));
    }

    [Test]
    public void LocatorInvokesReportOnProgressContextWhenProgressInstanceAvailable()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new FilterOptions { HasText = "Password" };

        htmlLocator.First();
        Mock.Get(htmlLocator.Context.Progress!).Invocations.Clear();

        // Act
        var newLocator = htmlLocator.Locator(XPathExpression.Compile("//label"), options);

        // Assert
        Mock.Get(htmlLocator.Context.Progress!).Verify(_m => _m.Report(It.IsAny<TestStep>()), Times.Once);
    }

    [Test]
    public void NthThrowsExceptionWhenIndexIsNegative()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => htmlLocator.Nth(-1));
        Assert.That(exception.Message, Is.EqualTo("Index must be a positive integer."));
    }

    [Test]
    public void NthThrowsExceptionWhenIndexIsGreaterThenCollectionCount()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => htmlLocator.Nth(5));
        Assert.That(exception.Message, Is.EqualTo(
            $"Expected to access the 5th index, but only 3 elements exist. This error occurred as part of validating " +
            $"HTML data."));
    }

    [Test]
    public void NthThrowsExceptionWhenCollectionIsEmpty()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 0);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => htmlLocator.Nth(0));
        Assert.That(exception.Message, Is.EqualTo(
            $"Expected to access the 0th index, but only 0 elements exist. This error occurred as part of validating " +
            $"HTML data."));
    }

    [Test]
    public void NthBuildsTestSession()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var index = 1;
        // Act
        htmlLocator.Nth(index);

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.Nth))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("index")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>($"{index}")))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("Nodes")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray())))),
                        Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nodes.Last().OriginalName.Trim())))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void NthInvokesReportOnProgressContextWhenProgressInstanceAvailable()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act
        var newLocator = htmlLocator.Nth(index: 1);

        // Assert
        Mock.Get(htmlLocator.Context.Progress!).Verify(_m => _m.Report(It.IsAny<TestStep>()), Times.Once);
    }

    [Test]
    public void HasCountBuildsTestSessionWhenPassedValidation()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var count = nodes.Count;
        
        // Act
        htmlLocator.HasCount(count);

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.HasCount))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("count")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>($"{count}")))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("Nodes")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray())))),
                        Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void HasCountBuildsTestSessionWhenNotPassedValidation()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var count = nodes.Count + 2;

        // Act
        Assert.Throws<ValidationException>(() => htmlLocator.HasCount(count));

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.HasCount))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("count")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>($"{count}")))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("Nodes")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray())))),
                        Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Never);
    }

    [Test]
    public void HasCountInvokesReportOnProgressContextWhenProgressInstanceAvailable()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());

        // Act
        htmlLocator.HasCount(count: 3);

        // Assert
        Mock.Get(htmlLocator.Context.Progress!).Verify(_m => _m.Report(It.IsAny<TestStep>()), Times.Once);
    }

    [Test]
    public void HasInnerTextBuildsTestSessionWhenPassedValidation()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new TextOptions() { Exact = true };
        htmlLocator.First();
        Mock.Get(htmlLocator.Context.SessionBuilder).Invocations.Clear();
        var innerText = "Password";

        // Act
        htmlLocator.HasInnerText(innerText, options);

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.HasInnerText))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("innerText")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(innerText)))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("TextOptions")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(options.ToString())))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>("label")))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public void HasInnerTextBuildsTestSessionWhenNotPassedValidation()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        var options = new TextOptions() { Exact = true };
        htmlLocator.First();
        Mock.Get(htmlLocator.Context.SessionBuilder).Invocations.Clear();
        var innerText = "notFound";

        // Act
        Assert.Throws<ValidationException>(() => htmlLocator.HasInnerText(innerText, options));

        // Assert
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("MethodName")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(nameof(htmlLocator.HasInnerText))))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("innerText")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(innerText)))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("TextOptions")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>(options.ToString())))), Times.Once);
        Mock.Get(htmlLocator.Context.SessionBuilder)
            .Verify(_m => _m.Build(
                It.Is<PropertyBagKey>(_p => _p == new PropertyBagKey("CurrentNode")),
                It.Is<IPropertyBagValue>(_p =>
                    _p.Equals(new PropertyBagValue<string>("label")))), Times.Once);
    }

    [Test]
    public void HasInnerTextInvokesReportOnProgressContextWhenProgressInstanceAvailable()
    {
        // Arrange
        var nodes = HtmlContentTestsHelpers.CreateHtmlNodeCollection(count: 3);
        var htmlLocator = new InstrumentedHtmlLocator(
            nodes: nodes,
            iterator: new HtmlNodeIterator(nodes),
            context: HtmlContentTestsHelpers.CreateTestContext());
        htmlLocator.First();
        Mock.Get(htmlLocator.Context.Progress!).Invocations.Clear();

        // Act
        htmlLocator.HasInnerText(innerText: "Password");

        // Assert
        Mock.Get(htmlLocator.Context.Progress!).Verify(_m => _m.Report(It.IsAny<TestStep>()), Times.Once);
    }
}
