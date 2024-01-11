using XPing365.Sdk.Core.Validators;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core;

public abstract class TestAgent(params TestStepHandler[] handlers)
{
    private readonly TestStepHandler[] _handlers = handlers ?? [];

    public IReadOnlyCollection<TestStepHandler> Handlers => _handlers;

    public virtual async Task<TestSession> RunAsync(
        Uri url,
        TestSettings settings,
        IValidator? validator = null,
        IProgress<TestStep>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(settings);

        var session = new TestSession(startDate: DateTime.UtcNow, url: url);

        foreach (TestStepHandler handler in _handlers)
        {
            TestStep testStep = await handler.HandleStepAsync(
                url,
                settings,
                session,
                cancellationToken).ConfigureAwait(false);

            if (testStep != null)
            {
                session.AddTestStep(testStep);
                progress?.Report(testStep);
            }
        }

        if (_handlers != null && _handlers.Length != 0 && validator != null)
        {
            await validator.ValidateAsync(url, settings, session, progress, cancellationToken).ConfigureAwait(false);
        }

        if (_handlers != null && _handlers.Length != 0)
        {
            session.Complete();
        }
        else
        {
            session.Decline(Errors.NoTestStepHandlers);
        }

        return await Task.FromResult(result: session).ConfigureAwait(false);
    }
}
