using XPing365.Sdk.Availability.Validations.Content.Html;
using XPing365.Sdk.Availability.Validations.Internals;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.HttpResponse.Internals;

internal class HttpHeaderValue(string header, IEnumerable<string> values) : IHttpHeaderValue
{
    private readonly string _header = header.RequireNotNull(nameof(header));
    private readonly IEnumerable<string> _values = values.RequireNotNull(nameof(values));

    public void HasValue(string value, TextOptions? options = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        var normalizedValue = value.Trim();

        if (!_values.Any(v => TextComparator.AreEqual(v, normalizedValue, options)))
        {
            throw new ValidationException(
                $"Expected to find HTTP header \"{_header}\" with value \"{normalizedValue}\", but the actual value" +
                $"was \"{string.Join(";", _values)}\". This exception occurred as part of validating HTTP response " +
                $"data.");
        }
    }
}
