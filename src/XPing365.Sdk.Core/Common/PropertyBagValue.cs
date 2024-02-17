using System.Runtime.Serialization;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.Common;

/// <summary>
/// Represents a serializable value that implements the <see cref="IPropertyBagValue"/> and <see cref="ISerializable"/> 
/// interfaces.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <remarks>
/// This class is used to store serializable values that are associated with test steps as outcomes. It supports four 
/// types of values: string, string[], byte[], and Dictionary&lt;string, string&gt;. It throws an ArgumentException 
/// during serialization process if the value to be serialized is not of one of these types.
/// </remarks>
[Serializable]
[KnownType(typeof(byte[]))]
[KnownType(typeof(string[]))]
[KnownType(typeof(Dictionary<string, string>))]
public sealed class PropertyBagValue<TValue> : IPropertyBagValue, ISerializable
{
    private static Type Type => typeof(TValue);

    /// <summary>
    /// Gets the value of the serializable property bag value.
    /// </summary>
    public TValue Value { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyBagValue{TValue}"/> class with the specified value.
    /// </summary>
    /// <param name="value">The value of the serializable property bag value.</param>
    public PropertyBagValue(TValue value)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyBagValue{TValue}"/> class with serialized data.
    /// </summary>
    /// <param name="info">
    /// The <see cref="SerializationInfo"/> that holds the serialized object data about the 
    /// exception being thrown.
    /// </param>
    /// <param name="context">
    /// The <see cref="StreamingContext"/> that contains contextual information about the source or destination.
    /// </param>
    public PropertyBagValue(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info, nameof(info));

        var type = Type
            .GetType(info.GetString(nameof(PropertyBagValue<TValue>.Type))
            .RequireNotNull(nameof(PropertyBagValue<TValue>.Type)));
        Value = (TValue)info.GetValue(nameof(Value), type!).RequireNotNull(nameof(Value));
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        // Null values are not permitted, since a null entry represents the absence of the key.
        if (Value is null)
        {
            return;
        }

        info.AddValue(nameof(PropertyBagValue<TValue>.Type), typeof(TValue).FullName);
        info.AddValue(nameof(Value), Value switch
        {
            string str => str,
            byte[] bytes => bytes,
            string[] array => array,
            Dictionary<string, string> dictionary => dictionary,
            _ => throw new ArgumentException(
                "Invalid type argument. Only byte[], string, string[] and Dictionary<string, string> are supported.")
        });
    }
}
