using HtmlAgilityPack;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;

internal abstract class AttributeSelector(string attribute) : ISelector
{
    private readonly string _name = attribute.RequireNotNullOrEmpty(nameof(attribute));

    public HtmlNodeCollection Select(HtmlNode node)
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));
        HtmlNodeCollection nodes = new(parentnode: node);

        foreach (HtmlNode n in node.SelectNodes(XPathExpressions.XPath(_name)))
        {
            var attrValue = n.Attributes[_name].Value.Trim();

            if (IsMatch(attrValue))
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }

    protected abstract bool IsMatch(string attributeValue);
}
