using System.Diagnostics;

namespace XPing365.Core;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class PropertyBagKey(string key) : IEquatable<PropertyBagKey?>
{
    private readonly string _key = key;

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

    public static bool operator ==(PropertyBagKey? left, PropertyBagKey? right)
    {
        return EqualityComparer<PropertyBagKey>.Default.Equals(left, right);
    }

    public static bool operator !=(PropertyBagKey? left, PropertyBagKey? right)
    {
        return !(left == right);
    }

    private string GetDebuggerDisplay()
    {
        return _key;
    }
}
