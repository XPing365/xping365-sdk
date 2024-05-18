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

internal class InstrumentedHtmlLocator : IHtmlLocator
{
    private readonly HtmlNodeCollection _nodes;
    private readonly IIterator<HtmlNode> _iterator;
    private readonly TestContext _context;

    public InstrumentedHtmlLocator(
        HtmlNodeCollection nodes,
        IIterator<HtmlNode> iterator,
        TestContext context)
    {
        _nodes = nodes.RequireNotNull(nameof(nodes));
        _iterator = iterator.RequireNotNull(nameof(iterator));
        _context = context.RequireNotNull(nameof(context));

        // Advance the iterator to the first item if the collection of nodes contains only one element.
        if (_nodes.Count == 1)
        {
            _iterator.First();
        }
    }

    public IHtmlLocator First()
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: nameof(First)),
                new PropertyBagValue<string>(_nodes.FirstOrDefault()?.OriginalName.Trim() ?? "Null"))
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        _iterator.First();

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return this;
    }

    public IHtmlLocator Last()
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: nameof(Last)),
                new PropertyBagValue<string>(_nodes.LastOrDefault()?.OriginalName.Trim() ?? "Null"))
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        _iterator.Last();

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return this;
    }

    public IHtmlLocator Filter(FilterOptions options)
    {
        _context.SessionBuilder.Build(
            new PropertyBagKey(key: nameof(Filter)),
            new PropertyBagValue<string>(options.ToString()));
            
        FilterSelector filterSelector = new(options);
        var newNodes = filterSelector.Select(_iterator.Current());

        _context.SessionBuilder.Build(
            new PropertyBagKey(key: "Nodes"),
            new PropertyBagValue<string[]>(newNodes.Select(n => n.OriginalName.Trim()).ToArray()));

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return new InstrumentedHtmlLocator(newNodes, new HtmlNodeIterator(newNodes), _context);
    }

    public IHtmlLocator Locator(XPathExpression selector, FilterOptions? options = null)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: nameof(Locator)),
                new PropertyBagValue<string>(selector.Expression))
            .Build(
                new PropertyBagKey(key: nameof(FilterOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"));

        XPathSelector xpathSelector = new(selector);
        var newNodes = xpathSelector.Select(_iterator.Current());

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(newNodes.Select(n => n.OriginalName.Trim()).ToArray()))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep); 

        return new InstrumentedHtmlLocator(newNodes, new HtmlNodeIterator(newNodes), _context);
    }

    public IHtmlLocator Nth(int index)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: nameof(Nth)),
                new PropertyBagValue<string>(index.ToString(CultureInfo.InvariantCulture)))
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        if (index < 0 || index >= _nodes.Count)
        {
            throw new ValidationException(
                $"Expected to access the {index}th index, but only {_nodes.Count} elements exist. This exception " +
                $"occurred as part of validating HTML data.");
        }

        _iterator.Nth(index);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return this;
    }

    public void Check()
    {
        throw new NotImplementedException();
    }

    public void Disabled()
    {
        throw new NotImplementedException();
    }

    public void HasCount(int count)
    {
        var expectedCount = count;
        var actualCount = _nodes.Count;

        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: nameof(HasCount)),
                new PropertyBagValue<string>(count.ToString(CultureInfo.InvariantCulture)))
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        if (actualCount != expectedCount)
        {
            throw new ValidationException(
                $"Expected to find {expectedCount} elements, but found {actualCount} instead. This exception " +
                $"occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public void HasInnerText(string innerText, TextOptions? options = default)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: nameof(HasInnerText)),
                new PropertyBagValue<string>(innerText))
            .Build(
                new PropertyBagKey(key: nameof(TextOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Default"))
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        if (_nodes.Count == 0)
        {
            throw new ValidationException(
                "No HTML nodes available. Ensure that the locator has selected at least one node before attempting " +
                "to validate inner text. This exception occurred as part of validating HTML data.");
        }

        if (!_iterator.IsAdvanced)
        {
            throw new ValidationException(
                "Multiple HTML nodes found. Please narrow down the results by calling 'First', 'Last', or 'Nth(int)' " +
                "properties or method on the HtmlLocator class before validating inner text. This exception occurred " +
                "as part of validating HTML data.");
        }

        var actualText = _iterator.Current().InnerText.Trim();

        if (!TextComparator.AreEqual(actualText, innerText, options))
        {
            throw new ValidationException(
                $"Expected the HTML node's inner text to be \"{innerText}\", but the actual inner text was " +
                $"\"{actualText}\". This exception occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public void HasInnerText(Regex innerText)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: nameof(HasInnerText)),
                new PropertyBagValue<string>(innerText.ToString()))
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        if (_nodes.Count == 0)
        {
            throw new ValidationException(
                "No HTML nodes available. Ensure that the locator has selected at least one node before attempting " +
                "to validate inner text. This exception occurred as part of validating HTML data.");
        }

        if (!_iterator.IsAdvanced)
        {
            throw new ValidationException(
                "Multiple HTML nodes found. Please narrow down the results by calling 'First', 'Last', or 'Nth(int)' " +
                "methods on the IHtmlLocator interface before validating inner text. This exception occurred as part " +
                "of validating HTML data.");
        }

        var actualText = _iterator.Current().InnerText.Trim();

        if (!innerText.IsMatch(actualText))
        {
            throw new ValidationException(
                $"Expected the HTML node's inner text to match \"{innerText}\" regex, but the actual inner text was " +
                $"\"{actualText}\". This exception occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public void HasInnerHtml(string innerHtml, TextOptions? options = null)
    {
        _context.SessionBuilder
           .Build(
               new PropertyBagKey(key: nameof(HasInnerHtml)),
               new PropertyBagValue<string>(innerHtml))
           .Build(
               new PropertyBagKey(key: nameof(TextOptions)),
               new PropertyBagValue<string>(options?.ToString() ?? "Default"))
           .Build(
               new PropertyBagKey(key: "Nodes"),
               new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        if (_nodes.Count == 0)
        {
            throw new ValidationException(
                "No HTML nodes available. Ensure that the locator has selected at least one node before attempting " +
                "to validate inner text. This exception occurred as part of validating HTML data.");
        }

        if (!_iterator.IsAdvanced)
        {
            throw new ValidationException(
                "Multiple HTML nodes found. Please narrow down the results by calling 'First', 'Last', or 'Nth(int)' " +
                "properties or method on the HtmlLocator class before validating inner text. This exception occurred " +
                "as part of validating HTML data.");
        }

        var actualHtml = _iterator.Current().InnerHtml.Trim();

        if (!TextComparator.AreEqual(actualHtml, innerHtml, options))
        {
            throw new ValidationException(
                $"Expected the HTML node's inner html to be \"{innerHtml}\", but the actual inner html was " +
                $"\"{actualHtml}\". This exception occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public void HasInnerHtml(Regex innerHtml)
    {
        _context.SessionBuilder
           .Build(
               new PropertyBagKey(key: nameof(HasInnerHtml)),
               new PropertyBagValue<string>(innerHtml.ToString()))
           .Build(
               new PropertyBagKey(key: "Nodes"),
               new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        if (_nodes.Count == 0)
        {
            throw new ValidationException(
                "No HTML nodes available. Ensure that the locator has selected at least one node before attempting " +
                "to validate inner text. This exception occurred as part of validating HTML data.");
        }

        if (!_iterator.IsAdvanced)
        {
            throw new ValidationException(
                "Multiple HTML nodes found. Please narrow down the results by calling 'First', 'Last', or 'Nth(int)' " +
                "properties or method on the HtmlLocator class before validating inner text. This exception occurred " +
                "as part of validating HTML data.");
        }

        var actualHtml = _iterator.Current().InnerHtml.Trim();

        if (!innerHtml.IsMatch(actualHtml))
        {
            throw new ValidationException(
                $"Expected the HTML node's inner html to match \"{innerHtml}\" regex, but the actual inner html was " +
                $"\"{actualHtml}\". This exception occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }
}
