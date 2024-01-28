using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace XPing365.Sdk.Common;

/// <summary>
/// This class provide extension methods to help verify parameters validity.
/// </summary>
public static class ArgumentValidation
{
    /// <summary>
    /// Basic Validation helper to verify parameter null validity.
    /// </summary>
    /// <typeparam name="T">The instance type</typeparam>
    /// <param name="obj">The parameter instance to verify</param>
    /// <param name="parameterName">The parameter name to verify</param>
    /// <returns>The instance that was passed to verify</returns>
    public static T RequireNotNull<T>(
        this T? obj, 
        string parameterName, 
        [CallerMemberName] string memberName = "", 
        [CallerFilePath] string sourceFilePath = "", 
        [CallerLineNumber] int sourceLineNumber = -1)
    {
        ArgumentNullException.ThrowIfNull(parameterName, nameof(parameterName));

        if (obj == null)
        {
            string errMsg = BuildErrorMessage(
                $"Argument {parameterName} is null.", memberName, sourceFilePath, sourceLineNumber);

            throw new ArgumentNullException(parameterName, errMsg);
        }

        return obj;
    }

    /// <summary>
    /// Verify validity of parameter instance through a condition.
    /// </summary>
    /// <typeparam name="T">The instance type</typeparam>
    /// <param name="obj">The parameter instance to verify</param>
    /// <param name="condition">The condition to test for</param>
    /// <param name="parameterName">The parameter name to verify</param>
    /// <param name="message">The message to post when the parameter instance fails validation</param>
    /// <returns>The instance that was passed to verify</returns>
    public static T RequireCondition<T>(
        this T obj,
        Func<T, bool> condition,
        string parameterName,
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = -1)
    {
        ArgumentException.ThrowIfNullOrEmpty(parameterName, nameof(parameterName));
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        ArgumentNullException.ThrowIfNull(obj, parameterName);
        ArgumentNullException.ThrowIfNull(condition, nameof(condition));

        if (!condition(obj))
        {
            string errMsg = BuildErrorMessage(message, memberName, sourceFilePath, sourceLineNumber);

            throw new ArgumentException(errMsg, parameterName);
        }

        return obj;
    }

    /// <summary>
    /// Verify string parameter to make sure it's not null or white space.
    /// </summary>
    /// <param name="obj">The parameter instance to verify</param>
    /// <param name="parameterName">The parameter name to verify</param>
    /// <returns>The instance that was passed to verify</returns>
    public static string RequireNotNullOrWhiteSpace(
        this string? obj,
        string parameterName,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = -1)
    {
        ArgumentException.ThrowIfNullOrEmpty(parameterName, nameof(parameterName));

        if (string.IsNullOrWhiteSpace(obj))
        {
            string errMsg = BuildErrorMessage(
                $"Argument {parameterName} is null or white space.", memberName, sourceFilePath, sourceLineNumber);

            throw new ArgumentException(errMsg, parameterName);
        }

        return obj;
    }

    /// <summary>
    /// Verify string parameter to make sure it's not null or empty.
    /// </summary>
    /// <param name="obj">The parameter instance to verify</param>
    /// <param name="parameterName">The parameter name to verify</param>
    /// <returns>The instance that was passed to verify</returns>
    public static string RequireNotNullOrEmpty(
        this string? obj,
        string parameterName,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = -1)
    {
        ArgumentException.ThrowIfNullOrEmpty(parameterName, nameof(parameterName));

        if (string.IsNullOrEmpty(obj))
        {
            string errMsg = BuildErrorMessage(
                $"Argument {parameterName} is null or empty.", memberName, sourceFilePath, sourceLineNumber);

            throw new ArgumentException(errMsg, parameterName);
        }

        return obj;
    }

    private static string BuildErrorMessage(string message, string? memberName, string? sourceFilePath, int sourceLineNumber)
    {
        var sb = new StringBuilder(message);

        if (!string.IsNullOrEmpty(memberName))
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, " CallerMemberName={0}", memberName);
        }

        if (!string.IsNullOrEmpty(sourceFilePath))
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, " CallerFilePath={0}", sourceFilePath);
        }

        if (sourceLineNumber >= 0)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, " CallerSourceLineNumber={0}", sourceLineNumber);
        }

        return sb.ToString();
    }
}
