using HtmlAgilityPack;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;

internal abstract class NodeSelector(XPath xpath) : ISelector
{
    private readonly XPath _xpath = xpath.RequireNotNull(nameof(xpath));

    public HtmlNodeCollection Select(HtmlNode node)
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));
        HtmlNodeCollection nodes = new(parentnode: node);

        foreach (HtmlNode n in node.SelectNodes(_xpath.Expression))
        {
            var nodeInnerText = n.InnerText.Trim();

            if (IsMatch(nodeInnerText))
            {
                nodes.Add(n);
            }
        }

        return nodes;
    }

    protected abstract bool IsMatch(string nodeInnerText);
}
