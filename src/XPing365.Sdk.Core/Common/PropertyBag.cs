using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace XPing365.Sdk.Core.Common;

/// <summary>
/// PropertyBag class represents a collection of key-value pairs that allows to store any object for a given unique key.
/// All keys are represented by <see cref="PropertyBagKey"/> but values may be of any type. Null values are not 
/// permitted, since a null entry represents the absence of the key.
/// </summary>
[Serializable]
public sealed class PropertyBag<TValue> : ISerializable
{
    private const string SerializableEntryName = "Properties";

    private readonly Dictionary<PropertyBagKey, TValue> _properties;

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    public int Count => _properties.Count;

    /// <summary>
    /// Gets a read-only collection that contains the keys of the collection.
    /// </summary>
    public ReadOnlyCollection<PropertyBagKey> Keys => _properties.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the PropertyBag class.
    /// </summary>
    /// <param name="properties">An optional dictionary of properties to initialize the property bag with.</param>
    public PropertyBag(IDictionary<PropertyBagKey, TValue>? properties = null)
    {
        _properties = properties?.ToDictionary() ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the PropertyBag class with serialized data.
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
    /// <param name="context">
    /// The StreamingContext that contains contextual information about the source or destination.
    /// </param>
    /// <remarks>
    /// The PropertyBag class implements the ISerializable interface and can be serialized and deserialized using binary 
    /// or XML formatters.
    /// </remarks>
    public PropertyBag(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info, nameof(info));

        if (info.GetValue(SerializableEntryName, typeof(Dictionary<PropertyBagKey, TValue>))
            is Dictionary<PropertyBagKey, TValue> properties)
        {
            _properties = properties;
        }
        else
        {
            _properties = [];
        }
    }

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
    public void AddOrUpdateProperties(IDictionary<PropertyBagKey, TValue> properties)
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
    public void AddOrUpdateProperty(PropertyBagKey key, TValue value)
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
    public bool TryGetProperty(PropertyBagKey key, out TValue? value)
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
    public TValue GetProperty(PropertyBagKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return _properties[key];
    }

    /// <summary>
    /// This method attempts to get the value associated with the specified key from the collection and cast it 
    /// to the specified type T.
    /// </summary>
    /// <typeparam name="T">The value type associated with the specified key.</typeparam>
    /// <param name="key">A key represented as <see cref="PropertyBagKey"/> type.</param>
    /// <param name="value">When this method returns, contains the value of specified <typeparamref name="T"/> type
    /// stored in the collection, if the key is found, or default value of <typeparamref name="T"/> if the key is
    /// not found.
    /// </param>
    /// <returns>true if a key was found successfully and its type matches with <typeparamref name="T"/>; 
    /// otherwise, false
    /// </returns>
    /// <remarks>This method does not throw exception when type of a value associated with a given key does not match 
    /// with <typeparamref name="T"/>.
    /// </remarks>
    public bool TryGetProperty<T>(PropertyBagKey key, out T? value) where T : TValue
    {
        value = default;

        // It is not expected to throw InvalidCastException when property cannot be cast to type T.
        if (TryGetProperty(key, out TValue? bag) && bag is T TProperty)
        {
            value = TProperty;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the value associated with the specified key from the collection and casts it to the specified type
    /// <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The value type associated with the specified key.</typeparam>
    /// <param name="key">A key represented as <see cref="PropertyBagKey"/> type.</param>
    /// <returns>The value associated with the specified key. If the specified key is not found, this operation
    /// throws a KeyNotFoundException exception. If the specified key is found, however its type does not match
    /// with <typeparamref name="T"/> it throws InvalidCastException.
    /// </returns>
    /// <exception cref="KeyNotFoundException">If the specified key is not found.</exception>
    /// <exception cref="InvalidCastException">If the specified value type <typeparamref name="T"/> does not match 
    /// with a value type stored in the collection.
    /// </exception>
    public T GetProperty<T>(PropertyBagKey key) where T : TValue
    {
        // It is expected to throw InvalidCastException when property cannot be cast to type T.
        return (T)GetProperty(key)!;
    }

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        _properties.Clear();
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info, nameof(info));

        var serializable = _properties
            .Where(item => item.Value is ISerializable)
            .ToDictionary(i => i.Key, i => i.Value);
        info.AddValue(SerializableEntryName, serializable);
    }
}
