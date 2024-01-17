using System.Net;
using XPing365.Sdk.Core;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestSteps;

/// <summary>
/// The DnsLookup class is a concrete implementation of the <see cref="TestStepHandler"/> class that is used to perform 
/// a DNS lookup. It uses the mechanisms provided by the operating system to perform DNS lookups.
/// </summary>
public sealed class DnsLookup() : TestStepHandler(StepName, TestStepType.ActionStep)
{
    public const string StepName = "DNS lookup";

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestSession"/> object that represents the test session.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    /// <returns><see cref="TestStep"/> object.</returns>
    public override async Task<TestStep> HandleStepAsync(
        Uri url, 
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));

        using var inst = new InstrumentationLog(startStopwatch: true);
        IPHostEntry resolved = await Dns.GetHostEntryAsync(url.Host, cancellationToken).ConfigureAwait(false);

        PropertyBag propertyBag = new();
        propertyBag.AddOrUpdateProperties(
            properties: new Dictionary<PropertyBagKey, object>()
            {
                { PropertyBagKeys.DnsResolvedIPAddresses, resolved.AddressList }
            });

        string errMsg = Errors.DnsLookupFailed;

        TestStep testStep = new(
            Name: StepName,
            StartDate: inst.StartTime,
            Duration: inst.ElapsedTime,
            Type: TestStepType.ActionStep,
            Result: resolved.AddressList.Length != 0 ? TestStepResult.Succeeded : TestStepResult.Failed,
            PropertyBag: propertyBag,
            ErrorMessage: resolved.AddressList.Length == 0 ? errMsg : null);

        return testStep;
    }
}
