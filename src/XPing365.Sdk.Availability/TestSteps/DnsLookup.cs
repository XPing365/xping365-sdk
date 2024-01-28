﻿using System.Net;
using XPing365.Sdk.Core;
using XPing365.Sdk.Common;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Availability.TestSteps;

/// <summary>
/// The DnsLookup class is a concrete implementation of the <see cref="TestComponent"/> class that is used to perform 
/// a DNS lookup. It uses the mechanisms provided by the operating system to perform DNS lookups.
/// </summary>
public sealed class DnsLookup() : TestComponent(StepName, TestStepType.ActionStep)
{
    public const string StepName = "DNS lookup";

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

        using var instrumentation = new InstrumentationLog(startStopwatch: true);
        TestStep testStep = null!;
        
        try
        {
            IPHostEntry resolved = await Dns.GetHostEntryAsync(url.Host, cancellationToken).ConfigureAwait(false);
            context.SessionBuilder.PropertyBag.AddOrUpdateProperty(
                PropertyBagKeys.DnsResolvedIPAddresses, resolved.AddressList);

            if (resolved != null && resolved.AddressList.Length != 0) 
            {
                testStep = context.SessionBuilder.Build(this, instrumentation);
            }
            else
            {
                testStep = context.SessionBuilder.Build(this, instrumentation, Errors.DnsLookupFailed);
            }
        }
        catch (Exception exception)
        {
            testStep = context.SessionBuilder.Build(this, instrumentation, exception);
        }
        finally
        {
            context.Progress?.Report(testStep);
        }
    }
}
