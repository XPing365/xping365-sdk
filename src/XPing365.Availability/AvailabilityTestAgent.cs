using XPing365.Availability.TestSteps;
using XPing365.Core;

namespace XPing365.Availability;

public sealed class AvailabilityTestAgent(
    IHttpClientFactory httpClientFactory) : ITestAgent
{
    private readonly TestStepHandler _handler = 
        new DnsLookup(
            new IPAddressAccessibilityCheck(
                new SendHttpRequest(httpClientFactory, successor: null)));

    public async Task<TestSession> RunAsync(
        Uri uri,
        TestSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(settings);

        var testSession = new TestSession(startDate: DateTime.UtcNow, url: uri);
        await _handler.HandleStepAsync(uri, settings, testSession, cancellationToken).ConfigureAwait(false);
        testSession.Complete();

        return testSession;
    }
}
