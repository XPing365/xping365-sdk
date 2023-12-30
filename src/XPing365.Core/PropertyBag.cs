namespace XPing365.Core;

public class PropertyBag
{
    private readonly Dictionary<PropertyBagKey, object> _properties;

    public int Count => _properties.Count;
    public IList<PropertyBagKey> Properties => _properties.Keys.ToList();

    public PropertyBag() => _properties = [];

    public void AddOrUpdateProperties(IDictionary<PropertyBagKey, object> properties)
    {
        if (properties == null)
        {
            return;
        }

        foreach (var property in properties)
        {
            AddOrUpdateProperties(property.Key, property.Value);
        }
    }

    public void AddOrUpdateProperties(PropertyBagKey key, object value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        if (!_properties.TryAdd(key, value))
        {
            _properties[key] = value;
        }
    }

    public bool TryGetProperty(PropertyBagKey key, out object? value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_properties.TryGetValue(key, out value))
        {
            return true;
        }

        return false;
    }

    public object GetProperty(PropertyBagKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return _properties[key];
    }

    public bool TryGetProperty<T>(PropertyBagKey key, out T? value)
    {
        value = default;

        // It is not expected to throw InvalidCastException when property cannot be cast to type T.
        if (TryGetProperty(key, out object? bag) && bag is T Tproperty)
        {
            value = Tproperty;
            return true;
        }

        return false;
    }

    public T GetProperty<T>(PropertyBagKey key)
    {
        // It is expected to throw InvalidCastException when property cannot be cast to type T.
        return (T)GetProperty(key);
    }

    public void Clear()
    {
        _properties.Clear();
    }
}
