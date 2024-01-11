using System.Net.NetworkInformation;
using XPing365.Sdk.Core;

namespace XPing365.Sdk.Availability.Extensions;

internal static class PingReplyExtension
{
    public static IDictionary<PropertyBagKey, object> ToProperties(this PingReply pingReply)
    {
        ArgumentNullException.ThrowIfNull(pingReply);

        Dictionary<PropertyBagKey, object> properties = new()
        {
            { PropertyBagKeys.IPStatus, pingReply.Status },
            { PropertyBagKeys.PingRoundtripTime, pingReply.RoundtripTime }
        };

        return properties;
    }
}
