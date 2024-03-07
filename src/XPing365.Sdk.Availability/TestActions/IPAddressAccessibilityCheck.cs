using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Availability.TestActions;

/// <summary>
/// The IPAddressAccessibilityCheck class is a concrete implementation of the <see cref="TestComponent"/> class that 
/// is used to check the accessibility of an IP address. It uses the mechanisms provided by the operating system to 
/// check the accessibility of an IP address.
/// <note>
/// The IPAddressAccessibilityCheck component requires the DnsLookup component to be registered before it in the 
/// pipeline, because it depends on the DNS resolution results.
/// </note>
/// </summary>
public sealed class IPAddressAccessibilityCheck() : TestComponent(name: StepName, type: TestStepType.ActionStep)
{
    /// <summary>
    /// The name of the test component that represents a IPAddressAccessibilityCheck of tests.
    /// </summary>
    /// <remarks>
    /// This constant is used to register the IPAddressAccessibilityCheck class in the test framework.
    /// </remarks>
    public const string StepName = "IPAddress accessibility check";

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="context">A <see cref="TestContext"/> object that represents the test context.</param>
    /// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    /// <returns><see cref="TestStep"/> object.</returns>
    public override async Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        ReadOnlyCollection<IPAddress>? addresses = GetIPAddresses(context.SessionBuilder.Steps);
        using var pingSender = new Ping();
        using var instrumentation = new InstrumentationLog(startStopwatch: true);
        bool completed = false;
        int addressIndex = 0;
        TestStep testStep = null!;

        try
        {
            if (addresses == null || addresses.Count == 0)
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

                    if (reply.Status == IPStatus.Success)
                    {
                        testStep = context.SessionBuilder
                            .Build(PropertyBagKeys.IPAddress, new PropertyBagValue<string>(address.ToString()))
                            .Build(component: this, instrumentation);
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
                } while (completed != true && addressIndex < addresses.Count);
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

    private static ReadOnlyCollection<IPAddress>? GetIPAddresses(ReadOnlyCollection<TestStep> steps)
    {
        PropertyBagValue<string[]>? bag = null;

        if (steps.Any(
            step => step.PropertyBag != null && 
            step.PropertyBag.TryGetProperty(key: PropertyBagKeys.DnsResolvedIPAddresses, value: out bag)) && 
            bag != null)
        {
            var result = bag.Value.Select(IPAddress.Parse).ToList();
            return result.AsReadOnly();
        }

        return null;
    }
}
