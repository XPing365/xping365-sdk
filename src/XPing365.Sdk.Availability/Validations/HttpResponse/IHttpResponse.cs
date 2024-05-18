using System.Net;
using XPing365.Sdk.Availability.Validations.Content.Html;

namespace XPing365.Sdk.Availability.Validations.HttpResponse;

/// <summary>
/// Represents an object for validating an HTTP response.
/// An instance of this interface will be passed to the validation function when used in conjunction with 
/// <see cref="HttpResponseValidator"/>.
/// </summary>
public interface IHttpResponse
{
    /// <summary>
    /// Retrieves the value of the specified HTTP header. 
    /// </summary>
    /// <param name="name">The name of the header to retrieve.</param>
    /// <param name="options">Optional text comparison options for header name matching.</param>
    /// <returns>An <see cref="IHttpHeaderValue"/> representing the header value.</returns>
    IHttpHeaderValue Header(string name, TextOptions? options = null);

    /// <summary>
    /// Validates that the HTTP response has the specified status code.
    /// </summary>
    /// <param name="statusCode">The expected status code.</param>
    void HasStatusCode(HttpStatusCode statusCode); 

    /// <summary>
    /// Ensures that the HTTP response has a successful (2xx) status code.
    /// </summary>
    void EnsureSuccessStatusCode();
}

