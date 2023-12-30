using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using XPing365.Availability.Extensions;
using XPing365.Core;

namespace XPing365.Availability.TestSteps;

internal sealed class IPAddressAccessibilityCheck(TestStepHandler? successor) : TestStepHandler(successor)
{
    // A buffer of 32 bytes of data to be transmitted during test.
    private readonly byte[] _buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

    public const string StepName = "IPAddress accessibility check";

    public override async Task HandleStepAsync(
        Uri uri,
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        IPAddress[]? addresses = GetIPAddresses(session);

        if (addresses != null && addresses.Length > 0)
        {
            using var pingSender = new Ping();
            using var instrumentation = new InstrumentationLog(startStopper: true);
            bool completed = false;
            int addressIndex = 0;
            
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

                    // Create PropertyBag from Ping reply and add IP Address on which the test step has been invoked.
                    var propertyBag = new PropertyBag();
                    propertyBag.AddOrUpdateProperties(reply.ToProperties());
                    propertyBag.AddOrUpdateProperties(PropertyBagKeys.IPAddress, address);

                    var testStep = new TestStep(
                        Name: StepName,
                        StartDate: instrumentation.StartTime,
                        Duration: instrumentation.ElapsedTime,
                        Type: TestStepType.ActionStep,
                        Result: reply.Status == IPStatus.Success ? TestStepResult.Succeeded : TestStepResult.Failed,
                        PropertyBag: propertyBag,
                        ErrorMessage: reply.Status != IPStatus.Success ? reply.Status.GetMessage(address) : null);
                    session.AddTestStep(testStep);

                    // This step does not iterate over all available IP addresses and completes immediately when 
                    // no exception has been thrown during accessibility check.
                    completed = true;
                }
                catch (PingException e)
                {
                    var testStep = TestStep.CreateActionStepFromException(StepName, e, instrumentation);
                    session.AddTestStep(testStep);
                }
            }
            while (completed != true && addressIndex < addresses.Length);
        }

        await base.HandleStepAsync(uri, settings, session, cancellationToken).ConfigureAwait(false);
    }

    private static PingOptions GetOptions(TestSettings settings) => new()
    {
        DontFragment = settings.PropertyBag.GetProperty<bool>(PropertyBagKeys.PingDontFragmetOption),
        Ttl = settings.PropertyBag.GetProperty<int>(PropertyBagKeys.PingTTLOption)
    };

    private static int GetTimeout(TestSettings settings) => (int)settings.Timeout.TotalMilliseconds;

    private static IPAddress[]? GetIPAddresses(TestSession session)
    {
        IPAddress[]? addresses = null;
        session.Steps?.Any(step => 
            step.PropertyBag.TryGetProperty(PropertyBagKeys.DnsResolvedIPAddresses, out addresses));

        return addresses;
    }
}
