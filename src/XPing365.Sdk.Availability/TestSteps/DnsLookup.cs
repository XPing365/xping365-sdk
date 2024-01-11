using System.Net;
using XPing365.Sdk.Core;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestSteps;

internal sealed class DnsLookup() : TestStepHandler(StepName, TestStepType.ActionStep)
{
    public const string StepName = "DNS lookup";

    public override async Task<TestStep> HandleStepAsync(
        Uri uri, 
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        using var inst = new InstrumentationLog(startStopwatch: true);
        IPHostEntry resolved = await Dns.GetHostEntryAsync(uri.Host, cancellationToken).ConfigureAwait(false);

        PropertyBag propertyBag = new();
        propertyBag.AddOrUpdateProperties(
            properties: new Dictionary<PropertyBagKey, object>()
            {
                { PropertyBagKeys.DnsResolvedIPAddresses, resolved.AddressList }
            });

        TestStep testStep = new(
            Name: StepName,
            StartDate: inst.StartTime,
            Duration: inst.ElapsedTime,
            Type: TestStepType.ActionStep,
            Result: resolved.AddressList.Length != 0 ? TestStepResult.Succeeded : TestStepResult.Failed,
            PropertyBag: propertyBag,
            ErrorMessage: resolved.AddressList.Length == 0 ?
                "Could not resolve the hostname to any IP address." : null);

        return testStep;
    }
}
