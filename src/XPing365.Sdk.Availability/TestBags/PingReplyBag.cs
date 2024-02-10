using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestBags;

[Serializable]
public sealed class PingReplyBag : ISerializable
{
    public IPStatus Status { get; }

    public IPAddress Address { get; }

    public long RoundtripTime { get; }

    public PingReplyBag(PingReply pingReply)
    {
        ArgumentNullException.ThrowIfNull(pingReply, nameof(pingReply));

        Status = pingReply.Status;
        Address = pingReply.Address;
        RoundtripTime = pingReply.RoundtripTime;
    }

    public PingReplyBag(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info, nameof(info));

        Status = Enum.Parse<IPStatus>(
            value: (string)info.GetValue(nameof(Status), typeof(string)).RequireNotNull(nameof(Status)),
            ignoreCase: true);
        Address = IPAddress.Parse(
            ipString: (string)info.GetValue(nameof(Address), typeof(string)).RequireNotNull(nameof(Address)));
        RoundtripTime = (long)info.GetValue(nameof(RoundtripTime), typeof(long)).RequireNotNull(nameof(RoundtripTime));
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Status), Status.ToString(), typeof(string));
        info.AddValue(nameof(Address), Address.ToString(), typeof(string));
        info.AddValue(nameof(RoundtripTime), RoundtripTime, typeof(long));
    }
}
