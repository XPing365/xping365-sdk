using System.Collections.ObjectModel;

namespace XPing365.Sdk.Core.Common;

/// <summary>
/// PropertyBag class represents a collection of key-value pairs that allows to store any object for a given unique key.
/// All keys are represented by <see cref="PropertyBagKey"/> but values may be of any type. Null values are not 
/// permitted, since a null entry represents the absence of the key.
/// </summary>
public class PropertyBag(IDictionary<PropertyBagKey, object>? properties = null)
{
    private readonly Dictionary<PropertyBagKey, object> _properties = properties?.ToDictionary() ?? [];

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    public int Count => _properties.Count;

    /// <summary>
    /// Gets a read-only collection that contains the keys of the collection.
    /// </summary>
    public ReadOnlyCollection<PropertyBagKey> Keys => _properties.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Determines whether the collection contains an element that has the specified key.
    /// </summary>
    /// <param name="key">A key represented as <see cref="PropertyBagKey"/> type.</param>
    /// <returns>Boolean value determining whether the key is found in a collection.</returns>
    public bool ContainsKey(PropertyBagKey key) => _properties.ContainsKey(key);

    /// <summary>
    /// Adds or updates a collection of key-value pairs to the collection.
    /// </summary>
    /// <param name="properties">A collection of a key-value pairs, where key is represented as 
    /// <see cref="PropertyBagKey"/> type.</param>
    /// <remarks>
    /// If a key is not found in the collection, a new key-value pair is created. If a key is already present in the
    /// collection, the value associated with the key is updated.
    /// </remarks>
    public void AddOrUpdateProperties(IDictionary<PropertyBagKey, object> properties)
    {
        ArgumentNullException.ThrowIfNull(properties, nameof(properties));

        foreach (var property in properties)
        {
            AddOrUpdateProperty(property.Key, property.Value);
        }
    }

    /// <summary>
    /// Adds or updates a key-value pair to the collection.
    /// </summary>
    /// <param name="key">A key represented as <see cref="PropertyBagKey"/> type.</param>
    /// <param name="value">Any value which should be stored in a collection.</param>
    /// <remarks>
    /// If a key is not found in the collection, a new key-value pair is created. If a key is already present in the
    /// collection, the value associated with the key is updated.
    /// </remarks>
    public void AddOrUpdateProperty(PropertyBagKey key, object value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        if (!_properties.TryAdd(key, value))
        {
            _properties[key] = value;
        }
    }

    /// <summary>
    /// This method attempts to get the value associated with the specified key from the collection.
    /// </summary>
    /// <param name="key">A key represented as <see cref="PropertyBagKey"/> type.</param>
    /// <param name="value">When this method returns, contains the object value stored in the collection, if
    /// the key is found, or null if the key is not found.</param>
    /// <returns>true if a key was found successfully; otherwise, false</returns>
    public bool TryGetProperty(PropertyBagKey key, out object? value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_properties.TryGetValue(key, out value))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the value associated with the specified key from the collection.
    /// </summary>
    /// <param name="key">A key represented as <see cref="PropertyBagKey"/> type.</param>
    /// <returns>The value associated with the specified key. If the specified key is not found, this operation
    /// throws a KeyNotFoundException exception.</returns>
    /// <exception cref="KeyNotFoundException">If the specified key is not found.</exception>
    public object GetProperty(PropertyBagKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return _properties[key];
    }

    /// <summary>
    /// This method attempts to get the value associated with the specified key from the collection and cast it 
    /// to the specified type T.
    /// </summary>
    /// <typeparam name="TValue">The value type associated with the specified key.</typeparam>
    /// <param name="key">A key represented as <see cref="PropertyBagKey"/> type.</param>
    /// <param name="value">When this method returns, contains the value of specified <typeparamref name="TValue"/> type
    /// stored in the collection, if the key is found, or default value of <typeparamref name="TValue"/> if the key is
    /// not found.
    /// </param>
    /// <returns>true if a key was found successfully and its type matches with <typeparamref name="TValue"/>; 
    /// otherwise, false
    /// </returns>
    /// <remarks>This method does not throw exception when type of a value associated with a given key does not match 
    /// with <typeparamref name="TValue"/>.
    /// </remarks>
    public bool TryGetProperty<TValue>(PropertyBagKey key, out TValue? value)
    {
        value = default;

        // It is not expected to throw InvalidCastException when property cannot be cast to type T.
        if (TryGetProperty(key, out object? bag) && bag is TValue TProperty)
        {
            value = TProperty;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the value associated with the specified key from the collection and casts it to the specified type
    /// <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The value type associated with the specified key.</typeparam>
    /// <param name="key">A key represented as <see cref="PropertyBagKey"/> type.</param>
    /// <returns>The value associated with the specified key. If the specified key is not found, this operation
    /// throws a KeyNotFoundException exception. If the specified key is found, however its type does not match
    /// with <typeparamref name="TValue"/> it throws InvalidCastException.
    /// </returns>
    /// <exception cref="KeyNotFoundException">If the specified key is not found.</exception>
    /// <exception cref="InvalidCastException">If the specified value type <typeparamref name="TValue"/> does not match 
    /// with a value type stored in the collection.
    /// </exception>
    public TValue GetProperty<TValue>(PropertyBagKey key)
    {
        // It is expected to throw InvalidCastException when property cannot be cast to type T.
        return (TValue)GetProperty(key);
    }

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        _properties.Clear();
    }
}
