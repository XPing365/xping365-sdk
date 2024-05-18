using HtmlAgilityPack;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals;

internal class HtmlNodeIterator(HtmlNodeCollection nodes) : IIterator<HtmlNode>
{
    private readonly HtmlNodeCollection _nodes = nodes.RequireNotNull(nameof(nodes));
    private int _cursor = -1;

    public bool IsAdvanced => _cursor >= 0;

    public HtmlNode Current()
    {
        if (_nodes.Count == 0)
        {
            throw new InvalidOperationException(
                "The collection of HTML nodes is empty. There are no elements to iterate over.");
        }

        if (_cursor == -1)
        {
            throw new InvalidOperationException(
                "The iterator has not been advanced. Call the 'First', 'Last' or 'Nth' methods to advance the " +
                "iterator before calling 'Current'.");
        }

        return _nodes[_cursor];
    }

    public void First()
    {
        if (_nodes.Count > 0)
        {
            _cursor = 0;
        }
    }

    public void Last()
    {
        _cursor = _nodes.Count - 1;
    }

    public void Nth(int index)
    {
        if (index > 0 && index < _nodes.Count)
        {
            _cursor = index;
        }
    }
}
