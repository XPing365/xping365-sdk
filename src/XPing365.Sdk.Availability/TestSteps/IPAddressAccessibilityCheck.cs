using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using XPing365.Sdk.Availability.TestSteps.Internals;
using XPing365.Sdk.Core;
using XPing365.Sdk.Common;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Availability.TestSteps;

/// <summary>
/// The IPAddressAccessibilityCheck class is a concrete implementation of the <see cref="TestComponent"/> class that 
/// is used to check the accessibility of an IP address. It uses the mechanisms provided by the operating system to 
/// check the accessibility of an IP address.
/// </summary>
public sealed class IPAddressAccessibilityCheck() : TestComponent(StepName, TestStepType.ActionStep)
{
    public const string StepName = "IPAddress accessibility check";

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestContext"/> object that represents the test session.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    /// <returns><see cref="TestStep"/> object.</returns>
    public override async Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        IPAddress[]? addresses = GetIPAddresses(context.SessionBuilder.PropertyBag);
        using var pingSender = new Ping();
        using var instrumentation = new InstrumentationLog(startStopwatch: true);
        bool completed = false;
        int addressIndex = 0;
        TestStep testStep = null!;

        try
        {
            if (addresses == null || addresses.Length == 0)
            {
                testStep = context.SessionBuilder.Build(
                    component: this, instrumentation, Errors.InsufficientData(component: this));
            }
            else
            {
                do
                {
                    IPAddress address = addresses[addressIndex++];
                    PingReply reply = await pingSender.SendPingAsync(
                        address: address,
                        timeout: GetTimeout(settings),
                        // On Linux, non-privileged processes can't send a custom payload. Please leave this empty.
                        buffer: [],
                        options: GetOptions(settings)).ConfigureAwait(false);

                    context.SessionBuilder.PropertyBag.AddOrUpdateProperties(reply.ToProperties());
                    context.SessionBuilder.PropertyBag.AddOrUpdateProperty(PropertyBagKeys.IPAddress, address);

                    if (reply.Status == IPStatus.Success)
                    {
                        testStep = context.SessionBuilder.Build(component: this, instrumentation);
                        completed = true;
                    }
                    else
                    {
                        testStep = context.SessionBuilder.Build(
                            component: this, instrumentation, Errors.PingRequestFailed);
                        completed = false;
                    }

                    // This step implements a fail-fast mechanism that terminates the iteration as soon as the first
                    // IP address passes. If an error occurs, the iteration continues with the next IP address.
                } while (completed != true && addressIndex < addresses.Length);
            }
        }
        catch (Exception exception)
        {
            testStep = context.SessionBuilder.Build(component: this, instrumentation, exception);
        }
        finally
        {
            context.Progress?.Report(testStep);
        }
    }

    private static PingOptions GetOptions(TestSettings settings) => new()
    {
        DontFragment = settings.PropertyBag.GetProperty<bool>(PropertyBagKeys.PingDontFragmetOption),
        Ttl = settings.PropertyBag.GetProperty<int>(PropertyBagKeys.PingTTLOption)
    };

    private static int GetTimeout(TestSettings settings) =>
        (int)settings.PropertyBag.GetProperty<TimeSpan>(PropertyBagKeys.HttpRequestTimeout).TotalMilliseconds;

    private static IPAddress[]? GetIPAddresses(PropertyBag propertyBag)
    {
        propertyBag.TryGetProperty(PropertyBagKeys.DnsResolvedIPAddresses, out IPAddress[]? addresses);
        return addresses;
    }
}
