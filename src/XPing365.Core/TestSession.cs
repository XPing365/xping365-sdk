namespace XPing365.Core;

public class TestSession(DateTime startDate, Uri url)
{
    private readonly List<TestStep> _steps = [];
    private TestSessionState _state = TestSessionState.NotStarted;

    public DateTime StartDate { get; } = startDate;
    public TimeSpan Duration => _steps.Aggregate(TimeSpan.Zero, (elapsedTime, step) => elapsedTime + step.Duration);
    public Uri Url { get; } = url;
    public TestSessionState State => _state;
    public IReadOnlyCollection<TestStep> Steps => _steps.AsReadOnly();

    public void AddTestStep(TestStep step)
    {
        ArgumentNullException.ThrowIfNull(step, nameof(step));
        _steps.Add(step);
    }

    public void Complete() => _state = TestSessionState.Completed;
}
