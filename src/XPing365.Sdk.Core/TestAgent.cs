using XPing365.Sdk.Core.Validators;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core;

/// <summary>
/// This class is used to perform testing operations. As a base and abstract class, it provides a set of common
/// properties and methods that are shared by all derived classes. The derived classes then implement their own 
/// specific functionality by overriding the base class methods.
/// </summary>
/// <param name="handlers">An array of <see cref="TestStepHandler"/> objects which will be used to perform specific
/// test operation.
/// </param>
public abstract class TestAgent(params TestStepHandler[] handlers)
{
    private readonly TestStepHandler[] _handlers = handlers ?? [];

    /// <summary>
    /// Gets a read-only collection of associated <see cref="TestStepHandler"/> objects.
    /// </summary>
    public IReadOnlyCollection<TestStepHandler> Handlers => _handlers;

    /// <summary>
    /// The RunAsync method executes two stages to generate a <see cref="TestSession"/> object. In the first stage, it 
    /// executes <see cref="TestStepHandler"/> objects that are defined in the derived classes. In the second stage, it 
    /// executes an optional validator object that is used to validate the test session coming from the previous stage.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="validator">An optional IValidator object that can be used to validate the test session.</param>
    /// <param name="progress">An optional IProgress&lt;TestStep&gt; object that can be used to report progress during
    /// the validation process.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// validation process.</param>
    /// <returns>
    /// Returns a Task&lt;TestStession&gt; object that represents the asynchronous result of testing operations.
    /// </returns>
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
