namespace XPing365.Sdk.Core.Common;

/// <summary>
/// Represents a non-serializable value that implements the <see cref="IPropertyBagValue"/> interface.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <remarks>
/// This class is used to store any value that should be excluded from being serialized during the 
/// <see cref="PropertyBag"/> serialization process. Its main purpose is to transfer data among different objects that 
/// do not need this data to be serialized.
/// </remarks>
public sealed class NonSerializable<TValue>(TValue value) : IPropertyBagValue
{
    /// <summary>
    /// Gets the value of the non-serializable property bag value.
    /// </summary>
    /// <value>
    /// The value of the non-serializable property bag value.
    /// </value>
    public TValue Value { get; init; } = value;
}