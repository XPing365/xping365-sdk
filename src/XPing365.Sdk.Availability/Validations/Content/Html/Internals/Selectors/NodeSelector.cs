using HtmlAgilityPack;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;

internal abstract class NodeSelector(string name) : ISelector
{
    private string _name = name.RequireNotNullOrEmpty(nameof(name));

    public HtmlNodeCollection Select(HtmlNode node)
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        HtmlNodeCollection nodes = new(parentnode: node);

        foreach (HtmlNode n in node.SelectNodes(XPathExpressions.XPath(_name)))
        {
            var nodeInnerText = n.InnerText.Trim();

            if (IsMatch(nodeInnerText))
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }

    protected abstract bool IsMatch(string nodeInnerText);
}
