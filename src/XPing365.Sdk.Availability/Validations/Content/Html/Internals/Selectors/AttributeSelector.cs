using HtmlAgilityPack;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;

internal abstract class AttributeSelector(XPath expression) : ISelector
{
    private readonly XPath _xpath = expression.RequireNotNull(nameof(expression));

    public HtmlNodeCollection Select(HtmlNode node)
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));
        HtmlNodeCollection nodes = new(parentnode: node);

        foreach (HtmlNode n in node.SelectNodes(_xpath.Expression))
        {
            var attrValue = n.Attributes[_xpath.Name].Value.Trim();

            if (IsMatch(attrValue))
            {
                nodes.Add(n);
            }
        }

        return nodes;
    }

    protected abstract bool IsMatch(string attributeValue);
}
