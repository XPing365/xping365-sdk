using System.Net;
using System.Net.Http.Headers;
using XPing365.Sdk.Availability.Validations.Content.Html;
using XPing365.Sdk.Availability.Validations.Internals;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.HttpResponse.Internals;

internal class HttpResponseInfo(HttpResponseMessage response, TestContext context) : IHttpResponse
{
    private readonly HttpResponseMessage _response = response.RequireNotNull(nameof(response));
    private readonly TestContext _context = context.RequireNotNull(nameof(context));

    public void EnsureSuccessStatusCode()
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(EnsureSuccessStatusCode)));

        if (!_response.IsSuccessStatusCode)
        {
            throw new ValidationException("" +
                $"Expected success status code, but the actual status code was \"{(int)_response.StatusCode}\". This " +
                $"exception occurred as part of validating HTTP response data.");
        }

        // Create a successful test step with detailed information about the current test operation.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public void HasStatusCode(HttpStatusCode statusCode)
    {
        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(HasStatusCode)))
            .Build(
                new PropertyBagKey(key: nameof(statusCode)),
                new PropertyBagValue<string>(statusCode.ToString()));

        if (statusCode != _response.StatusCode)
        {
            throw new ValidationException("" +
                $"Expected \"{(int)statusCode}\" status code, but the actual status code was " +
                $"\"{(int)_response.StatusCode}\". This exception occurred as part of validating HTTP response data.");
        }

        // Create a successful test step with detailed information about the current test operation.
        var testStep = _context.SessionBuilder.Build();
        // Report the progress of this test step.
        _context.Progress?.Report(testStep);
    }

    public IHttpHeaderValue Header(string name, TextOptions? options = null)
    {
        // Normalize header name
        var normalizedName = name.ToUpperInvariant().Trim();

        _context.SessionBuilder
            .Build(
                new PropertyBagKey(key: "MethodName"),
                new PropertyBagValue<string>(nameof(Header)))
            .Build(
                new PropertyBagKey(key: nameof(name)),
                new PropertyBagValue<string>(name))
            .Build(
                new PropertyBagKey(key: nameof(normalizedName)),
                new PropertyBagValue<string>(normalizedName))
            .Build(
                new PropertyBagKey(key: nameof(TextOptions)),
                new PropertyBagValue<string>(options?.ToString() ?? "Null"));

        var normalizedHeaders = ConcatenateDictionaries(
            GetNormalizedHeaders(_response.Headers),
            GetNormalizedHeaders(_response.Content.Headers),
            GetNormalizedHeaders(_response.TrailingHeaders));

        if (TryGetHeaderValue(normalizedHeaders, normalizedName, out var value, options) && value != null)
        {
            // Create a successful test step with detailed information about the current test operation.
            var testStep = _context.SessionBuilder.Build();
            // Report the progress of this test step.
            _context.Progress?.Report(testStep);

            return value;
        }

        throw new ValidationException(
            $"Expected to find HTTP header \"{normalizedName}\", but no such header exists. This exception occurred " +
            $"as part of validating HTTP response data.");
    }

    private static Dictionary<string, IEnumerable<string>> GetNormalizedHeaders(HttpHeaders headers) =>
        headers.ToDictionary(h => h.Key.ToUpperInvariant(), h => h.Value);

    private static bool TryGetHeaderValue(
        Dictionary<string, IEnumerable<string>> headers,
        string name,
        out HttpHeaderValue? value,
        TextOptions? options = null)
    {
        value = null;

        foreach (var header in headers)
        {
            if (TextComparator.AreEqual(header.Key, name, options))
            {
                value = new HttpHeaderValue(name, header.Value);
                return true;
            }
        }

        return false;
    }

    private static Dictionary<string, IEnumerable<string>> ConcatenateDictionaries(
        params Dictionary<string, IEnumerable<string>>[] dictionaries)
    {
        var result = new Dictionary<string, IEnumerable<string>>();

        foreach (var dict in dictionaries)
        {
            foreach (var kvp in dict)
            {
                if (result.TryGetValue(kvp.Key, out IEnumerable<string>? value))
                {
                    // Merge the values if the key already exists
                    result[kvp.Key] = value.Concat(kvp.Value);
                }
                else
                {
                    // Add the key-value pair if it doesn't exist
                    result.Add(kvp.Key, kvp.Value);
                }
            }
        }

        return result;
    }
}
