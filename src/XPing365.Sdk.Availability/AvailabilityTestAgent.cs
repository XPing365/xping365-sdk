using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Core;

namespace XPing365.Sdk.Availability;

public sealed class AvailabilityTestAgent(
    IHttpClientFactory httpClientFactory) : 
    TestAgent([
        new DnsLookup(), 
        new IPAddressAccessibilityCheck(), 
        new SendHttpRequest(httpClientFactory)])
{ }
