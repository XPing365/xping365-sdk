using System.Collections.ObjectModel;

namespace XPing365.Sdk.Core;

/// <summary>
/// PropertyBag class represents a collection of name value pairs
/// that allows to store any object for a given unique key. All 
/// keys are represented by PropertyBagKey but values may be of any type. 
/// Null values are not permitted, since a null entry represents the 
/// absence of the key.
/// </summary>
public class PropertyBag(IDictionary<PropertyBagKey, object>? properties = null)
{
    private readonly Dictionary<PropertyBagKey, object> _properties = properties?.ToDictionary() ?? [];

    public int Count => _properties.Count;
    public ReadOnlyCollection<PropertyBagKey> Keys => _properties.Keys.ToList().AsReadOnly();

    public bool ContainsKey(PropertyBagKey key) => _properties.ContainsKey(key);

    public void AddOrUpdateProperties(IDictionary<PropertyBagKey, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties, nameof(properties));

        foreach (var property in properties)
        {
            AddOrUpdateProperty(property.Key, property.Value);
        }
    }

    public void AddOrUpdateProperty(PropertyBagKey key, object value)
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
