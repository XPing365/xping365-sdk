using System.Text.RegularExpressions;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;

internal class AttributeRegexSelector(string attribute, Regex textRegex) :
    AttributeSelector(attribute)
{
    private readonly Regex _textRegex = textRegex.RequireNotNull(nameof(textRegex));

    protected override bool IsMatch(string attributeValue)
    {
        return _textRegex.IsMatch(attributeValue);
    }
}
