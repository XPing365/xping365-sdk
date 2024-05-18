using XPing365.Sdk.Availability.Validations.Internals;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;

internal class NodeTextSelector(string name, string text, TextOptions? options = null) : NodeSelector(name)
{
    private readonly string _text = text.RequireNotNullOrEmpty(nameof(text));
    private readonly TextOptions? _options = options;

    protected override bool IsMatch(string nodeInnerText)
    {
        return TextComparator.AreEqual(nodeInnerText, _text, _options);
    }
}
