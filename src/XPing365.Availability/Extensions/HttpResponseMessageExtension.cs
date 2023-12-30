﻿using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using XPing365.Core;

namespace XPing365.Availability.Extensions;

internal static class HttpResponseMessageExtension
{
    public static IDictionary<PropertyBagKey, object> ToProperties(this HttpResponseMessage httpResponse)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);

        Dictionary<PropertyBagKey, object> properties = new()
        {
            { PropertyBagKeys.HttpStatus, httpResponse.StatusCode },
            { PropertyBagKeys.HttpReasonPhrase, httpResponse.ReasonPhrase ?? string.Empty },
            { PropertyBagKeys.HttpVersion, httpResponse.Version },
            { PropertyBagKeys.HttpHeaders, DumpHeaders(httpResponse.Headers, httpResponse.TrailingHeaders) }
        };

        return properties;
    }

    public static string DumpHeaders(params HttpHeaders[] headers)
    {
        // Return all headers as string similar to: 
        // {"HeaderName1": "Value1", "HeaderName1": "Value2", "HeaderName2": "Value1"}
        
        StringBuilder sb = new();
        sb.Append('{');

        bool firstItem = true;

        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i] != null)
            {
                foreach (var header in headers[i])
                {
                    foreach (var headerValue in header.Value)
                    {
                        if (firstItem != true)
                        {
                            sb.Append(',');
                        }
                        
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", header.Key);
                        sb.Append(": ");
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", headerValue);
                        firstItem = false;
                    }
                }
            }
        }

        sb.Append('}');

        return sb.ToString();
    }
}
