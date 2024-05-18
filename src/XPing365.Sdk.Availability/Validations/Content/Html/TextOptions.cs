namespace XPing365.Sdk.Availability.Validations.Content.Html;

/// <summary>
/// Encapsulates options for text matching in HTML element location. This class provides configuration for text-based 
/// queries, allowing for precise or flexible matching criteria.
/// </summary>
public class TextOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to perform an exact match of the text. An exact match is case-sensitive 
    /// and matches the entire string. The default value is false. Note: Even with an exact match, leading and trailing 
    /// whitespace is trimmed.
    /// </summary>
    public bool Exact { get; set; }

    /// <summary>
    /// Returns a string that represents the current TextOptions object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"{nameof(Exact)}:{Exact}";
    }
}
