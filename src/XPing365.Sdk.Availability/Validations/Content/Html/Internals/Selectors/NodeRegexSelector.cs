﻿using System.Text.RegularExpressions;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;

internal class NodeRegexSelector(XPath xpath, Regex textRegex) : NodeSelector(xpath)
{
    private readonly Regex _textRegex = textRegex.RequireNotNull(nameof(textRegex));

    protected override bool IsMatch(string nodeInnerText)
    {
        return _textRegex.IsMatch(nodeInnerText);
    }
}
