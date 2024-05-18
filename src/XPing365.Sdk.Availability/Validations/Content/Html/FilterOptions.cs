using System.Text.RegularExpressions;

namespace XPing365.Sdk.Availability.Validations.Content.Html;

/// <summary>
/// Represents a set of filtering criteria used to refine the selection of HTML elements.
/// </summary>
public class FilterOptions
{
    /// <summary>
    /// Matches elements that do not contain specified text somewhere inside, possibly in a child or a descendant 
    /// element. Matching is case-insensitive and searches for a substring.
    /// </summary>
    public string? HasNotText { get; set; }

    /// <summary>
    /// Matches elements that do not contain specified text somewhere inside, possibly in a child or a descendant 
    /// element. When passed a <see cref="string"/>, matching is case-insensitive and searches for a substring.
    /// </summary>
    public Regex? HasNotTextRegex { get; set; }

    /// <summary>
    /// Matches elements containing specified text somewhere inside, possibly in a child or a descendant element. 
    /// Matching is case-insensitive and searches for a substring. For example, 
    /// <c>"Some Text"</c> matches <c>&lt;article&gt;&lt;div&gt;Some Text&lt;/div&gt;&lt;/article&gt;</c>.
    /// </summary>
    public string? HasText { get; set; }

    /// <summary>
    /// Matches elements containing specified text somewhere inside, possibly in a child or a descendant element. When 
    /// passed a <see cref="string"/>, matching is case-insensitive and searches for a substring. For example, 
    /// <c>"Some Text"</c> matches <c>&lt;article&gt;&lt;div&gt;Some Text&lt;/div&gt;&lt;/article&gt;</c>.
    /// </summary>
    public Regex? HasTextRegex { get; set; }
    
    /// <summary>
    /// Returns a string that represents the current TextOptions object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return 
            $"{nameof(HasNotText)}={HasNotText};" +
            $"{nameof(HasNotTextRegex)}={HasNotTextRegex?.ToString()};" +
            $"{nameof(HasText)}={HasText};" +
            $"{nameof(HasTextRegex)}={HasTextRegex?.ToString()};";
    }
}
