using System.Collections;

namespace XPing365.Sdk.Availability.TestActions.Internals;

internal class OrderedHttpRedirections : IEnumerable<string>
{
    private readonly HashSet<string> _hashSet = [];
    private readonly List<string> _insertionOrder = [];

    public int Count => _hashSet.Count;

    // Add items to HashSet and track order of insertion
    public bool Add(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        if (_hashSet.Add(url))
        {
            _insertionOrder.Add(url);
            return true;
        }

        return false;
    }

    // Find first item matching a condition starting from the end
    public string? FindLastMatchingItem(Func<string, bool> matchCondition)
    {
        for (int i = _insertionOrder.Count - 1; i >= 0; i--)
        {
            if (matchCondition(_insertionOrder[i]))
            {
                return _insertionOrder[i];
            }
        }

        return null;
    }

    public void Clear()
    {
        _hashSet.Clear();
        _insertionOrder.Clear();
    }

    public IEnumerator<string> GetEnumerator()
    {
        return ((IEnumerable<string>)this._insertionOrder).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)this._insertionOrder).GetEnumerator();
    }
}
