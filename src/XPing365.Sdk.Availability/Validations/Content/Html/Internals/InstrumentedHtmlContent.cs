using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using HtmlAgilityPack;
using XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;
using XPing365.Sdk.Availability.Validations.Internals;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals;

internal class InstrumentedHtmlContent : IHtmlContent
{
    private readonly TestContext _context;
    private readonly HtmlDocument _document;
    private readonly string _testIdAttribute;

    public TestContext Context => _context;
    public HtmlDocument Document => _document;

    public InstrumentedHtmlContent(string data, TestContext context, string testIdAttribute)
    {
        _document = new HtmlDocument();
        _document.LoadHtml(data.RequireNotNull(nameof(data)));
        _context = context.RequireNotNull(nameof(context));
        _testIdAttribute = testIdAttribute.RequireNotNullOrEmpty(nameof(testIdAttribute));
    }

    public void HasTitle(string title, TextOptions? options = null)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(HasTitle)))
            .Build(
                new PropertyBagKey(key: nameof(title)),
                new PropertyBagValue<string>(title))
            .Build(
                new PropertyBagKey(key: nameof(TextOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"));

        var selector = CreateByTitleSelector();
        var nodes = selector.Select(_document.DocumentNode);

        _context.SessionBuilder.Build(
            new PropertyBagKey(key: "Nodes"),
            new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        if (nodes.Count == 0)
        {
            throw new ValidationException(
                "No <title> node available. Ensure that the html content has <title> node before attempting " +
                "to validate its value. This error occurred as part of validating HTML data.");
        }

        if (nodes.Count > 1)
        {
            throw new ValidationException(
                "Multiple <title> nodes were found. The method expects a single <title> node under the <head> " +
                "node. Please ensure that the HTML content contains only one <title> node for proper validation. " +
                "This error occurred as part of validating HTML data.");
        }

        // The method expects a single <title> node under the <head> node.
        var actualText = nodes.First().InnerText.Trim();

        if (!TextComparator.AreEqual(actualText, title, options))
        {
            throw new ValidationException(
                $"Expected the <title> node's inner text to be \"{title}\", but the actual <title> node's inner " +
                $"text was \"{actualText}\". This error occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public void HasMaxDocumentSize(int maxSizeInBytes)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(HasMaxDocumentSize)))
            .Build(
                new PropertyBagKey(key: nameof(maxSizeInBytes)),
                new PropertyBagValue<string>(maxSizeInBytes.ToString(CultureInfo.InvariantCulture)));

        int byteCount = _document.Encoding.GetByteCount(_document.Text);

        _context.SessionBuilder.Build(
            new PropertyBagKey(key: "SizeInBytes"),
            new PropertyBagValue<string>(byteCount.ToString(CultureInfo.InvariantCulture)));

        if (byteCount > maxSizeInBytes)
        {
            throw new ValidationException(
                $"The expected HTML document size should be equal to or less than {maxSizeInBytes} bytes; however, " +
                $"the actual size was {byteCount} bytes. This error occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public IHtmlLocator GetByAltText(string text, TextOptions? options = null)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(GetByAltText)))
            .Build(
                new PropertyBagKey(key: nameof(text)),
                new PropertyBagValue<string>(text))
            .Build(
                new PropertyBagKey(key: nameof(TextOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"));

        var selector = CreateByAltTextSelector(text, options);
        var nodes = selector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    public IHtmlLocator GetByAltText(Regex text)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(GetByAltText)))
            .Build(
                new PropertyBagKey(key: nameof(text)),
                new PropertyBagValue<string>(text.ToString()));

        var selector = CreateByAltRegexSelector(text);
        var nodes = selector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    public IHtmlLocator GetByLabel(string text, TextOptions? options = null)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(GetByLabel)))
            .Build(
                new PropertyBagKey(key: nameof(text)),
                new PropertyBagValue<string>(text.ToString()))
            .Build(
                new PropertyBagKey(key: nameof(TextOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"));

        var selector = CreateByLabelTextSelector(text);
        var nodes = selector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    public IHtmlLocator GetByLabel(Regex text)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(GetByLabel)))
            .Build(
                new PropertyBagKey(key: nameof(text)),
                new PropertyBagValue<string>(text.ToString()));

        var selector = CreateByLabelRegexSelector(text);
        var nodes = selector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    public IHtmlLocator GetByPlaceholder(string text, TextOptions? options = null)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(GetByPlaceholder)))
            .Build(
                new PropertyBagKey(key: nameof(text)),
                new PropertyBagValue<string>(text.ToString()))
            .Build(
                new PropertyBagKey(key: nameof(TextOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"));

        var selector = CreateByPlaceholderTextSelector(text, options);
        var nodes = selector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    public IHtmlLocator GetByPlaceholder(Regex text)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(GetByPlaceholder)))
            .Build(
                new PropertyBagKey(key: nameof(text)),
                new PropertyBagValue<string>(text.ToString()));

        var selector = CreateByPlaceholderRegexSelector(text);
        var nodes = selector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    public IHtmlLocator GetByTestId(string text, TextOptions? options = null)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(GetByTestId)))
            .Build(
                new PropertyBagKey(key: nameof(text)),
                new PropertyBagValue<string>(text))
            .Build(
                new PropertyBagKey(key: nameof(TextOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"));

        var selector = CreateByTestIdTextSelector(text, options);
        var nodes = selector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    public IHtmlLocator GetByTestId(Regex text)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(GetByTestId)))
            .Build(
                new PropertyBagKey(key: nameof(text)),
                new PropertyBagValue<string>(text.ToString()));

        var selector = CreateByTestIdRegexSelector(text);
        var nodes = selector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    public IHtmlLocator GetByTitle(string text, TextOptions? options = null)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(GetByTitle)))
            .Build(
                new PropertyBagKey(key: nameof(text)),
                new PropertyBagValue<string>(text))
            .Build(
                new PropertyBagKey(key: nameof(TextOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"));

        var selector = CreateByTitleTextSelector(text, options);
        var nodes = selector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    public IHtmlLocator GetByTitle(Regex text)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(GetByTitle)))
            .Build(
                new PropertyBagKey(key: nameof(text)),
                new PropertyBagValue<string>(text.ToString()));

        var selector = CreateByTitleRegexSelector(text);
        var nodes = selector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    public IHtmlLocator Locator(XPathExpression selector, FilterOptions? options = null)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(Locator)))
            .Build(
                new PropertyBagKey(key: nameof(selector)),
                new PropertyBagValue<string>(selector.ToString() ?? "Null"))
            .Build(
                new PropertyBagKey(key: nameof(FilterOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"));

        XPathSelector xpathSelector = new(selector);
        var nodes = xpathSelector.Select(_document.DocumentNode);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(nodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(nodes, new HtmlNodeIterator(nodes), _context);
    }

    protected virtual ISelector CreateByTitleSelector()
    {
        return new XPathSelector(XPaths.Title.Expression);
    }

    protected virtual ISelector CreateByAltTextSelector(string text, TextOptions? options = null)
    {
        return new AttributeTextSelector(XPaths.Alt, text, options);
    }

    protected virtual ISelector CreateByAltRegexSelector(Regex text)
    {
        return new AttributeRegexSelector(XPaths.Alt, text);
    }

    protected virtual ISelector CreateByLabelTextSelector(string text, TextOptions? options = null)
    {
        return new NodeTextSelector(XPaths.Label, text, options);
    }

    protected virtual ISelector CreateByLabelRegexSelector(Regex text)
    {
        return new NodeRegexSelector(XPaths.Label, text);
    }

    protected virtual ISelector CreateByPlaceholderTextSelector(string text, TextOptions? options = null)
    {
        return new AttributeTextSelector(XPaths.Placeholder, text, options);
    }

    protected virtual ISelector CreateByPlaceholderRegexSelector(Regex text)
    {
        return new AttributeRegexSelector(XPaths.Placeholder, text);
    }

    protected virtual ISelector CreateByTestIdTextSelector(string text, TextOptions? options = null)
    {
        return new AttributeTextSelector(XPaths.TestIdAttribute(_testIdAttribute), text, options);
    }

    protected virtual ISelector CreateByTestIdRegexSelector(Regex text)
    {
        return new AttributeRegexSelector(XPaths.TestIdAttribute(_testIdAttribute), text);
    }

    protected virtual ISelector CreateByTitleTextSelector(string text, TextOptions? options = null)
    {
        return new AttributeTextSelector(XPaths.TitleAttribute, text, options);
    }

    protected virtual ISelector CreateByTitleRegexSelector(Regex text)
    {
        return new AttributeRegexSelector(XPaths.TitleAttribute, text);
    }
}
