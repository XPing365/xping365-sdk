using System.Diagnostics;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.Common;

/// <summary>
/// The Error class encapsulates the details of an error that occurs within the SDK. It has attributes: Code and 
/// Message. The Code represents a value that indicates the type of error, while the Message is a string that 
/// provides a human-readable description of the error. 
/// <note>
/// The Error class is intended to be used internally within the SDK.
/// </note>
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class Error(string code, string message) : IEquatable<Error>
{
    /// <summary>
    /// A static field that represents an empty error with no code or message.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Gets the string representation of the code that indicates the type of error.
    /// </summary>
    public string Code { get; } = code.RequireNotNullOrEmpty(nameof(code));

    /// <summary>
    /// Gets the string representation of the error that provides human-readable description.
    /// </summary>
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
