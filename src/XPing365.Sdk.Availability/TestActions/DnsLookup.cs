using System.Net;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Availability.TestActions;

/// <summary>
/// The DnsLookup class is a concrete implementation of the <see cref="TestComponent"/> class that is used to perform 
/// a DNS lookup. It uses the mechanisms provided by the operating system to perform DNS lookups.
/// </summary>
public sealed class DnsLookup() : TestComponent(name: StepName, type: TestStepType.ActionStep)
{
    public const string StepName = "DNS lookup";

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestContext"/> object that represents the test session.</param>
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

        using var instrumentation = new InstrumentationLog(startStopwatch: true);
        TestStep testStep = null!;
        
        try
        {
            IPHostEntry resolved = await Dns.GetHostEntryAsync(url.Host, cancellationToken).ConfigureAwait(false);

            if (resolved != null && resolved.AddressList != null && resolved.AddressList.Length != 0)
            {
                testStep = context.SessionBuilder
                    .Build(
                        key: PropertyBagKeys.DnsResolvedIPAddresses,
                        value: new PropertyBagValue<string[]>(
                            Array.ConvertAll(resolved.AddressList, addr => addr.ToString())))
                    .Build(component: this, instrumentation);
            }
            else
            {
                testStep = context.SessionBuilder.Build(component: this, instrumentation, Errors.DnsLookupFailed);
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
}
