﻿using System.Net;
using System.Net.Sockets;

namespace XPing365.Sdk.Availability.TestSteps.Internals;

internal static class IPAddressExtension
{
    public static bool IsIPv6(this IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(address, nameof(address));

        return address.AddressFamily == AddressFamily.InterNetworkV6;
    }
}