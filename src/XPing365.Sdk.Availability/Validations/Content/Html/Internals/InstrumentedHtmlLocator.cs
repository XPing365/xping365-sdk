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

    public TestContext Context => _context;

    public InstrumentedHtmlLocator(
        HtmlNodeCollection nodes,
        IIterator<HtmlNode> iterator,
        TestContext context)
    {
        _nodes = nodes.RequireNotNull(nameof(nodes));
        _iterator = iterator.RequireNotNull(nameof(iterator));
        _context = context.RequireNotNull(nameof(context));

        // If the collection of nodes has elements, advance the iterator to the first item. This allows validation
        // functions like HasInnerText to be called without needing to advance the iterator first.
        if (_nodes.Count >= 1)
        {
            _iterator.First();
        }
    }

    public IHtmlLocator First()
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(First)))
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        _iterator.First();

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "CurrentNode"),
                new PropertyBagValue<string>(_iterator.Current()?.OriginalName.Trim() ?? "Null"))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return this;
    }

    public IHtmlLocator Last()
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(Last)))
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        _iterator.Last();

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "CurrentNode"),
                new PropertyBagValue<string>(_iterator.Current()?.OriginalName.Trim() ?? "Null"))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return this;
    }

    public IHtmlLocator Filter(FilterOptions options)
    {
        var currentNode = _iterator.Current();

        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(Filter)))
            .Build(
                new PropertyBagKey(key: "CurrentNode"),
                new PropertyBagValue<string>(currentNode?.OriginalName.Trim() ?? "Null"))
            .Build(
                new PropertyBagKey(key: nameof(FilterOptions)),
                new PropertyBagValue<string>(options.ToString()));

        HtmlNodeCollection? filteredNodes = null;

        if (currentNode != null)
        {
            _context.SessionBuilder.Build(
                new PropertyBagKey(key: "ChildNodes"),
                new PropertyBagValue<string[]>(
                    currentNode.ChildNodes.Select(n => n.OriginalName.Trim()).ToArray()));

            FilterSelector filterSelector = new(options);
            filteredNodes = filterSelector.Select(currentNode);

            _context.SessionBuilder.Build(
                new PropertyBagKey(key: "FilteredNodes"),
                new PropertyBagValue<string[]>(filteredNodes.Select(n => n.OriginalName.Trim()).ToArray()));
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        if (filteredNodes != null && filteredNodes.Count > 0)
        {
            return new InstrumentedHtmlLocator(filteredNodes, new HtmlNodeIterator(filteredNodes), _context);
        }

        return this;
    }

    public IHtmlLocator Locator(XPathExpression selector, FilterOptions? options = null)
    {
        var currentNode = _iterator.Current();

        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(Locator)))
            .Build(
                new PropertyBagKey(key: "CurrentNode"),
                new PropertyBagValue<string>(currentNode?.OriginalName.Trim() ?? "Null"))
            .Build(
                new PropertyBagKey(key: nameof(selector)),
                new PropertyBagValue<string>(selector.Expression))
            .Build(
                new PropertyBagKey(key: nameof(FilterOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"));

        HtmlNodeCollection? locatedNodes = null;

        if (currentNode != null)
        {
            _context.SessionBuilder.Build(
                new PropertyBagKey(key: "ChildNodes"),
                new PropertyBagValue<string[]>(
                    currentNode.ChildNodes.Select(n => n.OriginalName.Trim()).ToArray()));

            XPathSelector xpathSelector = new(selector);
            locatedNodes = xpathSelector.Select(currentNode);

            _context.SessionBuilder
                .Build(
                    new PropertyBagKey(key: "LocatedNodes"),
                    new PropertyBagValue<string[]>(locatedNodes?.Select(n => n.OriginalName.Trim()).ToArray() ?? 
                        ["Null"]));
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        if (locatedNodes != null)
        {
            return new InstrumentedHtmlLocator(locatedNodes, new HtmlNodeIterator(locatedNodes), _context);
        }

        return this;
    }

    public IHtmlLocator Nth(int index)
    {
        if (index < 0)
        {
            throw new ArgumentException("Index must be a positive integer.");
        }

        static string FormatIndex(int index)
        {
            string suffix = (index % 10) switch
            {
                1 when index % 100 != 11 => "st",
                2 when index % 100 != 12 => "nd",
                3 when index % 100 != 13 => "rd",
                _ => "th",
            };
            return $"{index}{suffix}";
        }

        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(Nth)))
            .Build(
                new PropertyBagKey(key: nameof(index)),
                new PropertyBagValue<string>(index.ToString(CultureInfo.InvariantCulture)))
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        if (index >= _nodes.Count)
        {
            throw new ValidationException(
                $"Expected to access the {FormatIndex(index)} index, but only {_nodes.Count} elements exist." +
                $" This error occurred as part of validating HTML data.");
        }

        _iterator.Nth(index);

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "CurrentNode"),
                new PropertyBagValue<string>(_iterator.Current()?.OriginalName.Trim() ?? "Null"))
            .Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);

        return this;
    }

    public void HasCount(int count)
    {
        var expectedCount = count;
        var actualCount = _nodes.Count;

        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(HasCount)))
            .Build(
                new PropertyBagKey(key: nameof(count)),
                new PropertyBagValue<string>($"{count}"))
            .Build(
                new PropertyBagKey(key: "Nodes"),
                new PropertyBagValue<string[]>(_nodes.Select(n => n.OriginalName.Trim()).ToArray()));

        if (actualCount != expectedCount)
        {
            throw new ValidationException(
                $"Expected to find {expectedCount} elements, but found {actualCount} instead. This error " +
                $"occurred as part of validating HTML data.");
        } 

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public void HasInnerText(string innerText, TextOptions? options = default)
    {
        var currentNode = _iterator.Current();

        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(HasInnerText)))
            .Build(
                new PropertyBagKey(key: nameof(innerText)),
                new PropertyBagValue<string>(innerText))
            .Build(
                new PropertyBagKey(key: nameof(TextOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"))
            .Build(
                new PropertyBagKey(key: "CurrentNode"),
                new PropertyBagValue<string>(currentNode?.OriginalName.Trim() ?? "Null"));

        if (currentNode == null || _nodes.Count == 0)
        {
            throw new ValidationException(
                "No HTML nodes available. Ensure that the locator has selected at least one node before attempting " +
                "to validate inner text. This error occurred as part of validating HTML data.");
        }

        var actualText = currentNode.InnerText.Trim();

        if (!TextComparator.AreEqual(actualText, innerText, options))
        {
            throw new ValidationException(
                $"Expected the HTML node's inner text to be \"{innerText}\", but the actual inner text was " +
                $"\"{actualText}\". This error occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public void HasInnerText(Regex innerText)
    {
        var currentNode = _iterator.Current();

        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(HasInnerText)))
            .Build(
                new PropertyBagKey(key: nameof(innerText)),
                new PropertyBagValue<string>(innerText.ToString()))
            .Build(
                new PropertyBagKey(key: "CurrentNode"),
                new PropertyBagValue<string>(currentNode?.OriginalName.Trim() ?? "Null"));

        if (currentNode == null || _nodes.Count == 0)
        {
            throw new ValidationException(
                "No HTML nodes available. Ensure that the locator has selected at least one node before attempting " +
                "to validate inner text. This error occurred as part of validating HTML data.");
        }

        var actualText = currentNode.InnerText.Trim();

        if (!innerText.IsMatch(actualText))
        {
            throw new ValidationException(
                $"Expected the HTML node's inner text to match \"{innerText}\" regex, but the actual inner text was " +
                $"\"{actualText}\". This error occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public void HasInnerHtml(string innerHtml, TextOptions? options = null)
    {
        var currentNode = _iterator.Current();

        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(HasInnerHtml)))
            .Build(
                new PropertyBagKey(key: nameof(innerHtml)),
                new PropertyBagValue<string>(innerHtml))
            .Build(
                new PropertyBagKey(key: nameof(TextOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Default"))
            .Build(
                new PropertyBagKey(key: "CurrentNode"),
                new PropertyBagValue<string>(currentNode?.OriginalName.Trim() ?? "Null"));

        if (currentNode == null || _nodes.Count == 0)
        {
            throw new ValidationException(
                "No HTML nodes available. Ensure that the locator has selected at least one node before attempting " +
                "to validate inner text.  This error occurred as part of validating HTML data.");
        }

        var actualHtml = currentNode.InnerHtml.Trim();

        if (!TextComparator.AreEqual(actualHtml, innerHtml, options))
        {
            throw new ValidationException(
                $"Expected the HTML node's inner html to be \"{innerHtml}\", but the actual inner html was " +
                $"\"{actualHtml}\".  This error occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public void HasInnerHtml(Regex innerHtml)
    {
        var currentNode = _iterator.Current();

        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(HasInnerHtml)))
            .Build(
                new PropertyBagKey(key: nameof(innerHtml)),
                new PropertyBagValue<string>(innerHtml.ToString()))
            .Build(
                new PropertyBagKey(key: "CurrentNode"),
                new PropertyBagValue<string>(currentNode?.OriginalName.Trim() ?? "Null"));

        if (currentNode == null || _nodes.Count == 0)
        {
            throw new ValidationException(
                "No HTML nodes available. Ensure that the locator has selected at least one node before attempting " +
                "to validate inner text.  This error occurred as part of validating HTML data.");
        }

        var actualHtml = currentNode.InnerHtml.Trim();

        if (!innerHtml.IsMatch(actualHtml))
        {
            throw new ValidationException(
                $"Expected the HTML node's inner html to match \"{innerHtml}\" regex, but the actual inner html was " +
                $"\"{actualHtml}\".  This error occurred as part of validating HTML data.");
        }

        // Create a successful test step with detailed information about the current state of the HTML locator.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }
}
