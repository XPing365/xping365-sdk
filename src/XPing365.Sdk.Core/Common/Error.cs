using System.Diagnostics;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.Common;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class Error(string code, string message) : IEquatable<Error>
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public string Code { get; } = code.RequireNotNullOrEmpty(nameof(code));

    public string Message { get; } = message.RequireNotNullOrEmpty(nameof(message));

    public bool Equals(Error? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Code == null || other.Code == null)
        {
            return false;
        }

        return Code == other.Code;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Error);
    }

    public override int GetHashCode()
    {
        return string.GetHashCode(Code, StringComparison.InvariantCulture);
    }

    public static bool operator ==(Error? lhs, Error? rhs)
    {
        if (lhs is null || rhs is null)
        {
            return Equals(lhs, rhs);
        }

        return lhs.Equals(rhs);
    }

    public static bool operator !=(Error? lhs, Error? rhs)
    {
        if (lhs is null || rhs is null)
        {
            return !Equals(lhs, rhs);
        }

        return !lhs.Equals(rhs);
    }

    public static implicit operator string(Error error) => error.RequireNotNull(nameof(error)).ToString();

    public override string ToString()
    {
        return $"Error {Code}: {Message}";
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
