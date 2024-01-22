using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using XPing365.Sdk.Availability.TestSteps.Internals;
using XPing365.Sdk.Core;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestSteps;

/// <summary>
/// The IPAddressAccessibilityCheck class is a concrete implementation of the <see cref="TestStepHandler"/> class that 
/// is used to check the accessibility of an IP address. It uses the mechanisms provided by the operating system to 
/// check the accessibility of an IP address.
/// </summary>
public sealed class IPAddressAccessibilityCheck() : TestStepHandler(StepName, TestStepType.ActionStep)
{
    // A buffer of 32 bytes of data to be transmitted during test.
    private readonly byte[] _buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

    public const string StepName = "IPAddress accessibility check";

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestSession"/> object that represents the test session.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    /// <returns><see cref="TestStep"/> object.</returns>
    public override Task<TestStep> HandleStepAsync(
        Uri url,
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        IPAddress[]? addresses = GetIPAddresses(session);

        if (addresses == null || addresses.Length == 0)
        {
            return Task.FromResult(CreateFailedTestStep(Errors.InsufficientData(handler: this)));
        }

        using var pingSender = new Ping();
        using var inst = new InstrumentationLog(startStopwatch: true);
        bool completed = false;
        int addressIndex = 0;

        TestStep testStep;
        do
        {
            try
            {
                IPAddress address = addresses[addressIndex++];
                PingReply reply = pingSender.Send(
                    address: address,
                    timeout: GetTimeout(settings),
                    buffer: _buffer,
                    options: GetOptions(settings));

                var propertyBag = new PropertyBag(reply.ToProperties());
                // Add IP Address on which the test step has been invoked.
                propertyBag.AddOrUpdateProperty(PropertyBagKeys.IPAddress, address);

                if (reply.Status == IPStatus.Success)
                {
                    testStep = CreateSuccessTestStep(inst.StartTime, inst.ElapsedTime, propertyBag);
                }
                else
                {
                    testStep = CreateFailedTestStep(reply.Status.GetMessage(address));
                }

                // This step does not iterate over all available IP addresses and completes immediately when 
                // first IP address passes. Only on error it continues with next IP address.
                completed = true;
            }
            catch (Exception e)
            {
                testStep = CreateTestStepFromException(e, inst.StartTime, inst.ElapsedTime);
            }
        }
        while (completed != true && addressIndex < addresses.Length);

        return Task.FromResult(testStep);
    }

    private static PingOptions GetOptions(TestSettings settings) => new()
    {
        DontFragment = settings.PropertyBag.GetProperty<bool>(PropertyBagKeys.PingDontFragmetOption),
        Ttl = settings.PropertyBag.GetProperty<int>(PropertyBagKeys.PingTTLOption)
    };

    private static int GetTimeout(TestSettings settings) => 
        (int)settings.PropertyBag.GetProperty<TimeSpan>(PropertyBagKeys.HttpRequestTimeout).TotalMilliseconds;

    private static IPAddress[]? GetIPAddresses(TestSession session)
    {
        IPAddress[]? addresses = null;
        session.Steps?.Any(step =>
            step.PropertyBag.TryGetProperty(PropertyBagKeys.DnsResolvedIPAddresses, out addresses));

        return addresses;
    }
}
