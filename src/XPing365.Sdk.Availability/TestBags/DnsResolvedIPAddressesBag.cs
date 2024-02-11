using System.Net;
using XPing365.Sdk.Shared;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using XPing365.Sdk.Core.Common;

namespace XPing365.Sdk.Availability.TestBags;

[Serializable]
[KnownType(typeof(string[]))]
public sealed class DnsResolvedIPAddressesBag : ISerializable
{
    private readonly IPAddress[] _addresses;

    public static PropertyBagKey Key => new(nameof(DnsResolvedIPAddressesBag));

    public ReadOnlyCollection<IPAddress> IPAddresses => _addresses.AsReadOnly();

    public DnsResolvedIPAddressesBag(IPAddress[] addresses)
    {
        _addresses = addresses.RequireNotNull(nameof(addresses));
    }

    public DnsResolvedIPAddressesBag(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info);

        string[] addresses = info.GetValue(nameof(IPAddresses), typeof(string[])) as string[] ?? [];
        _addresses = addresses.Select(IPAddress.Parse).ToArray();
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        string[] adresses = _addresses.Select(addr => addr.ToString()).ToArray();
        info.AddValue(nameof(IPAddresses), adresses, typeof(string[]));
    }
}
