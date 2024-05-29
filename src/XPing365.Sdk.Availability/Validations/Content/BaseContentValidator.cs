using System.Net.Http.Headers;
using System.Text;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Availability.Validations.Content;

/// <summary>
/// Represents an abstract base class for validating HTTP content.
/// </summary>
/// <remarks>
/// The BaseContentValidator class inherits from the TestComponent class and provides a common method for decoding 
/// HTTP content.
/// </remarks>
public abstract class BaseContentValidator(string name) : TestComponent(name, TestStepType.ValidateStep)
{
    /// <summary>
    /// Decodes the HTTP content from a byte array and content headers.
    /// </summary>
    /// <param name="data">The byte array that contains the HTTP content.</param>
    /// <param name="contentHeaders">
    /// The content headers that specify the encoding and media type of the content.
    /// </param>
    /// <returns>A string representation of the HTTP content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the data or contentHeaders parameter is null.</exception>
    /// <exception cref="DecoderFallbackException">Thrown when a decoder fallback operation fails.</exception>
    protected virtual string GetContent(byte[] data, HttpContentHeaders contentHeaders)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));
        ArgumentNullException.ThrowIfNull(contentHeaders, nameof(contentHeaders));

        foreach (string encoding in contentHeaders.ContentEncoding)
        {
            try
            {
                string contentString = Encoding.GetEncoding(encoding).GetString(data);
                return contentString;
            }
            catch (Exception)
            {
                // Unable to decode content with this encoding, try the next one
            }
        }

        try
        {
            // Fallback to content-type header
            if (contentHeaders.ContentType?.CharSet != null)
            {
                return Encoding.GetEncoding(contentHeaders.ContentType.CharSet).GetString(data);
            }
        }
        catch (Exception)
        { }

        // Fallback to UTF-8
        return Encoding.UTF8.GetString(data);
    }
}
