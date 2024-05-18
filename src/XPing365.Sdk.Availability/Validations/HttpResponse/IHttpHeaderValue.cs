using XPing365.Sdk.Availability.Validations.Content.Html;

namespace XPing365.Sdk.Availability.Validations.HttpResponse;

/// <summary>
/// Represents a contract for validating HTTP header values.
/// </summary>
public interface IHttpHeaderValue
{
    /// <summary>
    /// Validates that the HTTP header value matches the specified expected value.
    /// </summary>
    /// <param name="value">The expected header value.</param>
    /// <param name="options">Optional text comparison options for value matching.</param>
    void HasValue(string value, TextOptions? options = null);
}


