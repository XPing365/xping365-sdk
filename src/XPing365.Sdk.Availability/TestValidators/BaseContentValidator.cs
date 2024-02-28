using System.Net.Http.Headers;
using System.Text;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Availability.TestValidators;

public abstract class BaseContentValidator(string name) : TestComponent(name, TestStepType.ValidateStep)
{
    protected static string GetContent(byte[] data, HttpContentHeaders contentHeaders)
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
