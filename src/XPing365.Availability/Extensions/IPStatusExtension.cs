using System.Net;
using System.Net.NetworkInformation;

namespace XPing365.Availability.Extensions;

public static class IPStatusExtension
{
    public static string GetMessage(this IPStatus status, IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(status, nameof(status));
        ArgumentNullException.ThrowIfNull(address, nameof(address));

        return status switch
        {
            IPStatus.Unknown => "The ICMP echo request failed for an unknown reason.",
            IPStatus.Success => "The ICMP echo request succeeded",
            IPStatus.DestinationNetworkUnreachable => "The ICMP echo request failed because the network that " +
                "contains the destination computer is not reachable.",
            IPStatus.DestinationHostUnreachable => "The ICMP echo request failed because the destination " +
                "computer is not reachable.",
            IPStatus.DestinationProhibited => address.IsIPv6() ?
                "The ICMPv6 echo request failed because contact with the destination " +
                "computer is administratively prohibited. This value applies only to IPv6. " : 
                "The ICMP echo request failed because the destination computer that is specified " +
                "in an ICMP echo message is not reachable, because it does not support the packet's " +
                "protocol. This value applies only to IPv4. This value is described in IETF RFC " +
                "1812 as Communication Administratively Prohibited.",
            IPStatus.DestinationPortUnreachable => "The ICMP echo request failed because the port on the " +
                "destination computer is not available.",
            IPStatus.NoResources => "The ICMP echo request failed because of insufficient network resources.",
            IPStatus.BadOption => "The ICMP echo request failed because it contains an invalid option.",
            IPStatus.HardwareError => "The ICMP echo request failed because of a hardware error.",
            IPStatus.PacketTooBig => 
                "The ICMP echo request failed because the packet containing the request is larger " +
                "than the maximum transmission unit (MTU) of a node (router or gateway) located " +
                "between the source and destination. The MTU defines the maximum size of a transmittable " +
                "packet.",
            IPStatus.TimedOut => "The ICMP echo Reply was not received within the allotted time.",
            IPStatus.BadRoute => 
                "The ICMP echo request failed because there is no valid route between the source " +
                "and destination computers.",
            IPStatus.TtlExpired => 
                "The ICMP echo request failed because its Time to Live (TTL) value reached zero, " +
                "causing the forwarding node (router or gateway) to discard the packet.",
            IPStatus.TtlReassemblyTimeExceeded => 
                "The ICMP echo request failed because the packet was divided into fragments for " +
                "transmission and all of the fragments were not received within the time allotted " +
                "for reassembly. RFC 2460 specifies 60 seconds as the time limit within which " +
                "all packet fragments must be received.",
            IPStatus.ParameterProblem =>
                "The ICMP echo request failed because a node (router or gateway) encountered problems " +
                "while processing the packet header. This is the status if, for example, the header " +
                "contains invalid field data or an unrecognized option.",
            IPStatus.SourceQuench =>
                "The ICMP echo request failed because the packet was discarded. This occurs when " +
                "the source computer's output queue has insufficient storage space, or when packets " +
                "arrive at the destination too quickly to be processed.",
            IPStatus.BadDestination =>
                "The ICMP echo request failed because the destination IP address cannot receive " +
                "ICMP echo requests or should never appear in the destination address field of " +
                "any IP datagram. For example, specifying IP address \"000.0.0.0\" returns this status.",
            IPStatus.DestinationUnreachable =>
                "The ICMP echo request failed because the destination computer that is specified " +
                "in an ICMP echo message is not reachable; the exact cause of problem is unknown.",
            IPStatus.TimeExceeded =>
                "The ICMP echo request failed because its Time to Live (TTL) value reached zero, " +
                "causing the forwarding node (router or gateway) to discard the packet.",
            IPStatus.BadHeader => "The ICMP echo request failed because the header is invalid.",
            IPStatus.UnrecognizedNextHeader => 
                "The ICMP echo request failed because the Next Header field does not contain a " +
                "recognized value. The Next Header field indicates the extension header type (if " +
                "present) or the protocol above the IP layer, for example, TCP or UDP. ",
            IPStatus.IcmpError => "The ICMP echo request failed because of an ICMP protocol error.",
            IPStatus.DestinationScopeMismatch =>
                "The ICMP echo request failed because the source address and destination address " +
                "that are specified in an ICMP echo message are not in the same scope. This is " +
                "typically caused by a router forwarding a packet using an interface that is outside " +
                "the scope of the source address. Address scopes (link-local, site-local, and " +
                "global scope) determine where on the network an address is valid.",
            _ => "The ICMP echo request failed for an unknown reason.",
        };
    }
}
