using HtmlAgilityPack;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals;

internal class HtmlNodeIterator(HtmlNodeCollection nodes) : IIterator<HtmlNode>
{
    private const int Uninitialized = -1;

    private readonly HtmlNodeCollection _nodes = nodes.RequireNotNull(nameof(nodes));
    private int _cursor = Uninitialized;

    public bool IsAdvanced => _cursor >= 0;
    internal int Cursor => _cursor;

    public HtmlNode? Current()
    {
        if (_nodes.Count == 0)
        {
            // "The collection of HTML nodes is empty. There are no elements to iterate over.");
            return null;
        }

        if (_cursor == Uninitialized)
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
        if (_nodes.Count > 0)
        {
            _cursor = _nodes.Count - 1;
        }
    }

    public void Nth(int index)
    {
        if (index >= 0 && index < _nodes.Count)
        {
            _cursor = index;
        }
    }
}
