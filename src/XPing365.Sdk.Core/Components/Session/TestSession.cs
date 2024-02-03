using System.Diagnostics;
using System.Globalization;
using System.Text;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Common;
using XPing365.Sdk.Core.Common;

namespace XPing365.Sdk.Core.Components.Session;

/// <summary>
/// This class is used to represent a test session. It provides a set of properties that can be used to access 
/// information about the test session, such as its start date, duration, URL, state, and steps.
/// </summary>
/// <param name="startDate">Represents the start date of the test session.</param>
/// <param name="url">A Uri object that represents the URL of the page being validated.</param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class TestSession
{
    private readonly List<TestStep> _steps;
    private TestSessionState _state = TestSessionState.NotStarted;

    public TestSession(Uri url, DateTime startDate, ICollection<TestStep> steps, PropertyBag propertyBag)
    {
        _steps = [.. steps];
        Url = url ?? throw new ArgumentNullException(nameof(url), Errors.MissingUrlInTestSession);
        PropertyBag = propertyBag ?? new PropertyBag();
        StartDate = startDate.RequireCondition(
            condition: date => date >= DateTime.Today.ToUniversalTime(),
            parameterName: nameof(StartDate),
            message: Errors.IncorrectStartDate);
    }

    private TestSession(string declinedReason)
    {
        _state = TestSessionState.Declined;
        _steps = null!;
        DeclineReason = declinedReason;
        StartDate = DateTime.MinValue;
        Url = null!;
        PropertyBag = null!;
    }

    public static TestSession GetDeclinedTestSession(string declinedReason)
    {
        return new TestSession(declinedReason);
    }

    /// <summary>
    /// Gets the start date of the test session.
    /// </summary>
    public DateTime StartDate { get; }

    /// <summary>
    /// Gets the total duration of the test session.
    /// </summary>
    public TimeSpan Duration => _steps.Aggregate(TimeSpan.Zero, (elapsedTime, step) => elapsedTime + step.Duration);

    /// <summary>
    /// A Uri object that represents the URL of the page being validated.
    /// </summary>
    public Uri Url { get; }

    /// <summary>
    /// Gets the state of the test session.
    /// </summary>
    public TestSessionState State => _state;

    /// <summary>
    /// Gets the property bag which stores custom key-value pairs from test component.
    /// </summary>
    public PropertyBag PropertyBag { get; }

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
            $"{StartDate} ({TotalTime(Duration)}) " +
            $"Test session {State.GetDisplayName()} for {Url.AbsoluteUri}." +
            $"{Environment.NewLine}");
        sb.AppendFormat(
            CultureInfo.InvariantCulture,
            $"Total steps: {Steps.Count}, Success: {Steps.Count - Failures.Count}, Failures: {Failures.Count}" +
            $"{Environment.NewLine}{Environment.NewLine}");

        return sb.ToString();
    }

    private string GetDebuggerDisplay() =>
        $"{StartDate} ({TotalTime(Duration)}), Steps: {_steps.Count}, Failures: {Failures.Count} ";

    private string TotalTime(TimeSpan duration)
    {
        if (duration.TotalSeconds >= 1)
        {
            return $"{Math.Round(Duration.TotalSeconds, 2)}[s]";
        }

        return $"{Math.Round(Duration.TotalMilliseconds, 0)}[ms]";
    }
}
