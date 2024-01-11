using System.Diagnostics;
using System.Globalization;
using System.Text;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core;

/// <summary>
/// This class is used to represent a test session. It provides a set of properties that can be used to access 
/// information about the test session, such as its start date, duration, URL, state, and steps.
/// </summary>
/// <param name="startDate">Represents the start date of the test session.</param>
/// <param name="url">Represents the URL of the server under tests.</param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class TestSession(DateTime startDate, Uri url)
{
    private readonly List<TestStep> _steps = [];
    private TestSessionState _state = TestSessionState.NotStarted;

    /// <summary>
    /// Gets the start date of the test session.
    /// </summary>
    public DateTime StartDate { get; } = startDate;

    /// <summary>
    /// Gets the total duration of the test session.
    /// </summary>
    public TimeSpan Duration => _steps.Aggregate(TimeSpan.Zero, (elapsedTime, step) => elapsedTime + step.Duration);

    /// <summary>
    /// Gets the URL of the server under tests.
    /// </summary>
    public Uri Url { get; } = url.RequireNotNull(nameof(url));

    /// <summary>
    /// Gets the state of the test session.
    /// </summary>
    public TestSessionState State => _state;

    /// <summary>
    /// Returns a boolean value indicating whether the test session is valid or not.
    /// Valid test session has all test steps completed successfully. Check <see cref="Failures"/> to get 
    /// failed test steps.
    /// </summary>
    public bool IsValid => _steps.Count > 0 && !_steps.Any(step => step.Result == TestStepResult.Failed);

    /// <summary>
    /// Returns a read-only collection of the test steps executed within current test session.
    /// </summary>
    public IReadOnlyCollection<TestStep> Steps => _steps.AsReadOnly();

    /// <summary>
    /// Returns a read-only collection of the failed test steps within current test session.
    /// </summary>
    public IReadOnlyCollection<TestStep> Failures =>
        _steps.Where(step => step.Result == TestStepResult.Failed).ToList().AsReadOnly();

    /// <summary>
    /// Returns decline reason for the current test session.
    /// </summary>
    public string? DeclineReason { get; internal set; }

    /// <summary>
    /// Internal use only, it adds a new <see cref="TestStep"/> object to the list of steps for the current 
    /// test session.
    /// </summary>
    public void AddTestStep(TestStep step)
    {
        ArgumentNullException.ThrowIfNull(step, nameof(step));
        _steps.Add(step);
    }

    /// <summary>
    /// Internal use only, it sets the state of the test session to Completed after all tests steps are run.
    /// </summary>
    public void Complete() => _state = TestSessionState.Completed;

    /// <summary>
    /// Internal use only, it sets the state of the test session to Declined state when declined by test agent.
    /// </summary>
    public void Decline(string declineReason)
    {
        ArgumentException.ThrowIfNullOrEmpty(declineReason, nameof(declineReason));

        _state = TestSessionState.Declined;
        DeclineReason = declineReason;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendFormat(
            CultureInfo.InvariantCulture, 
            $"{StartDate} ({Duration.TotalMilliseconds}[ms]) " +
            $"Test session {State.GetDisplayName()} for {Url.AbsoluteUri}." +
            $"{Environment.NewLine}");
        sb.AppendFormat(
            CultureInfo.InvariantCulture,
            $"Total steps: {Steps.Count}, Failures: {Failures.Count}" +
            $"{Environment.NewLine}{Environment.NewLine}");

        foreach (var step in _steps)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, step.ToString() + Environment.NewLine);
        }

        return sb.ToString();
    }

    private string GetDebuggerDisplay() => 
        $"{StartDate} ({Duration.TotalMilliseconds}[ms]), Steps: {_steps.Count}, Failures: {Failures.Count} ";
}
