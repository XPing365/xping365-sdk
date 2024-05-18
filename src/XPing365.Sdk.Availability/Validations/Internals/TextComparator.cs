using XPing365.Sdk.Availability.Validations.Content.Html;

namespace XPing365.Sdk.Availability.Validations.Internals;

internal static class TextComparator
{
    public static bool AreEqual(string a, string b, TextOptions? options = null)
    {
        if (options != null && options.Exact && a.Equals(b, StringComparison.Ordinal))
        {
            return true;
        }
        else if (a.Contains(b, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public static bool IsMatch(string text, FilterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        if (options.HasText != null && text.Equals(options.HasText, StringComparison.Ordinal))
        {
            return true;
        }
        else if (options.HasTextRegex != null && options.HasTextRegex.IsMatch(text))
        {
            return true;
        }
        else if (options.HasNotText != null && !text.Equals(options.HasNotText, StringComparison.Ordinal))
        {
            return true;
        }
        else if (options.HasNotTextRegex != null && !options.HasNotTextRegex.IsMatch(text))
        {
            return true;
        }

        return false;
    }
}
