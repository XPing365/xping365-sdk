using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using XPing365.Sdk.Availability.Extensions;
using XPing365.Sdk.Core;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestSteps;

internal sealed class IPAddressAccessibilityCheck() : TestStepHandler(StepName, TestStepType.ActionStep)
{
    // A buffer of 32 bytes of data to be transmitted during test.
    private readonly byte[] _buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

    public const string StepName = "IPAddress accessibility check";

    public override Task<TestStep> HandleStepAsync(
        Uri uri,
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
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
