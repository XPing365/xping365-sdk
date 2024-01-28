using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Availability;

/// <summary>
/// The AvailabilityTestAgent class is a concrete implementation of the <see cref="TestAgent"/> class that is used to 
/// perform availability tests. This class consist of following action test steps: 
/// <see cref="DnsLookup"/>, <see cref="IPAddressAccessibilityCheck"/> and <see cref="SendHttpRequest"/> to perform the 
/// availability tests. All action steps are performed in a specific order, and their results are added as 
/// <see cref="TestStep"/> results to the <see cref="TestContext"/> object. Any failures can be retrieved from the 
/// <see cref="TestContext.Failures"/> property along with the error description.
/// </summary>
/// <example>
/// <code>
/// using XPing365.Sdk.Availability;
/// 
/// Host.CreateDefaultBuilder()
///     .ConfigureServices(services =>
///     {
///         services.AddAvailabilityTestAgent();
///     });
/// 
/// var testAgent = _serviceProvider.GetRequiredService&lt;AvailabilityTestAgent&gt;();
/// 
/// TestContext session = await testAgent
///     .RunAsync(
///         new Uri("www.demoblaze.com"),
///         TestSettings.DefaultForAvailability)
///     .ConfigureAwait(false);
/// </code>
/// </example>
/// <param name="httpClientFactory"><see cref="IHttpClientFactory"/> implementation instance.</param>
public sealed class AvailabilityTestAgent(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider) : 
    TestAgent(serviceProvider, new Pipeline(name: PipelineName, [
        new DnsLookup(), 
        new IPAddressAccessibilityCheck(), 
        new SendHttpRequest(httpClientFactory)]))
{
    public const string PipelineName = "Availability pipeline";
}
