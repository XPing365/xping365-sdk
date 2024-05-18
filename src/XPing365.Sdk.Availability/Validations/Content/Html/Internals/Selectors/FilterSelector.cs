using HtmlAgilityPack;
using XPing365.Sdk.Availability.Validations.Internals;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;

internal class FilterSelector(FilterOptions options) : ISelector
{
    private readonly FilterOptions _options = options;

    public HtmlNodeCollection Select(HtmlNode node)
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));
        HtmlNodeCollection nodes = node.ChildNodes;

        foreach (HtmlNode n in nodes)
        {
            var nodeInnerText = n.InnerText.Trim();

            if (TextComparator.IsMatch(nodeInnerText, _options))
            {
                nodes.Add(n);
            }
        }

        return nodes;
    }
}
