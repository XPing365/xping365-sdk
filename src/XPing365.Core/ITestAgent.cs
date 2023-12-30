namespace XPing365.Core;

public interface ITestAgent
{
    public abstract Task<TestSession> RunAsync(
        Uri uri,
        TestSettings settings,
        CancellationToken cancellationToken = default);
}
