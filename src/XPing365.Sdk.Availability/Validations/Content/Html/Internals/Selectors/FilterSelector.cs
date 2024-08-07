using HtmlAgilityPack;
using XPing365.Sdk.Availability.Validations.Internals;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;

internal class FilterSelector(FilterOptions options) : ISelector
{
    private readonly FilterOptions _options = options;

    public HtmlNodeCollection Select(HtmlNode node)
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));
        HtmlNodeCollection filteredNodes = new(parentnode: node);

        foreach (HtmlNode n in node.ChildNodes)
        {
            var nodeInnerText = n.InnerText.Trim();

            if (TextComparator.IsMatch(nodeInnerText, _options))
            {
                filteredNodes.Add(n);
            }
        }

        return filteredNodes;
    }
}
