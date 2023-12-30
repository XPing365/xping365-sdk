using System.Net;
using XPing365.Core;

namespace XPing365.Availability.TestSteps;

internal sealed class DnsLookup(TestStepHandler? successor) : TestStepHandler(successor)
{
    public const string StepName = "DNS lookup";

    public override async Task HandleStepAsync(
        Uri uri, 
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        using var instrumentation = new InstrumentationLog(startStopper: true);
        IPHostEntry resolved = await Dns.GetHostEntryAsync(uri.Host, cancellationToken).ConfigureAwait(false);
        TimeSpan elapsedTime = instrumentation.ElapsedTime;

        PropertyBag propertyBag = new();
        propertyBag.AddOrUpdateProperties(
            properties: new Dictionary<PropertyBagKey, object>()
            {
                { PropertyBagKeys.DnsResolvedIPAddresses, resolved.AddressList }
            });

        TestStep testStep = new(
            Name: StepName,
            StartDate: instrumentation.StartTime,
            Duration: elapsedTime,
            Type: TestStepType.ActionStep,
            Result: resolved.AddressList.Length != 0 ? TestStepResult.Succeeded : TestStepResult.Failed,
            PropertyBag: propertyBag,
            ErrorMessage: resolved.AddressList.Length == 0 ?
                "Could not resolve the hostname to any IP address." : null);
        session.AddTestStep(testStep);

        await base.HandleStepAsync(uri, settings, session, cancellationToken).ConfigureAwait(false);
    }
}
