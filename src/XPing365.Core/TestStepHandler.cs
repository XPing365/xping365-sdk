namespace XPing365.Core;

public abstract class TestStepHandler
{
    private readonly TestStepHandler? _successor;

    protected TestStepHandler(TestStepHandler? successor)
    {
        this._successor = successor;
    }

    public virtual async Task HandleStepAsync(
        Uri uri,
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(settings);

        if (_successor != null)
        {
            await _successor.HandleStepAsync(uri, settings, session, cancellationToken).ConfigureAwait(false);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
