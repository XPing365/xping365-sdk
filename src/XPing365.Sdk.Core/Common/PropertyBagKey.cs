using System.Diagnostics;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.Common;

/// <summary>
/// The PropertyBagKey class is used to represent a key in a <see cref="PropertyBag"/>. 
/// The IEquatable&lt;PropertyBagKey?&gt; interface is implemented to allow for comparison of PropertyBagKey instances.
/// </summary>
/// <param name="key">
/// The key parameter represents the string value of the PropertyBagKey instance, which is used to 
/// identify this instance in a <see cref="PropertyBag"/>.
/// </param>
[Serializable]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class PropertyBagKey(string key) : IEquatable<PropertyBagKey?>
{
    private readonly string _key = key.RequireNotNullOrEmpty(nameof(key));

    public override bool Equals(object? obj)
    {
        return Equals(obj as PropertyBagKey);
    }

    public bool Equals(PropertyBagKey? other)
    {
        return other is not null && _key == other._key;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_key);
    }

    /// <summary>
    /// Overloads the == operator and returns a boolean value indicating whether the two specified PropertyBagKey 
    /// instances are equal.
    /// </summary>
    /// <param name="left">The left operand of the comparison operator</param>
    /// <param name="right">The right operand of the comparison operator</param>
    /// <returns>Boolean value that indicates whether the left parameter is equal to the right parameter</returns>
    public static bool operator ==(PropertyBagKey? left, PropertyBagKey? right)
    {
        return EqualityComparer<PropertyBagKey>.Default.Equals(left, right);
    }

    /// <summary>
    /// Overloads the != operator and returns a boolean value indicating whether the two specified PropertyBagKey 
    /// instances are not equal.
    /// </summary>
    /// <param name="left">The left operand of the comparison operator</param>
    /// <param name="right">The right operand of the comparison operator</param>
    /// <returns>Boolean value that indicates whether the left parameter is not equal to the right parameter</returns>
    public static bool operator !=(PropertyBagKey? left, PropertyBagKey? right)
    {
        return !(left == right);
    }

    public override string ToString() => _key;

    private string GetDebuggerDisplay() => _key;
}
