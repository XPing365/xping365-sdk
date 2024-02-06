using XPing365.Sdk.Core.Common;

namespace XPing365.Sdk.Core.Components.Session;

/// <summary>
/// TestSessionBuilder is a concrete implementation of the <see cref="ITestSessionBuilder"/> interface that is used to 
/// build test sessions.
/// </summary>
public class TestSessionBuilder : ITestSessionBuilder
{
    private readonly List<TestStep> _steps = [];
    private DateTime _startDate = DateTime.MinValue;
    private Uri? _url;

    /// <summary>
    /// Gets a value indicating whether the test session has failed.
    /// </summary>
    public bool HasFailed => _steps.Any(step => step.Result == TestStepResult.Failed);

    /// <summary>
    /// Gets the property bag that stores key-value pairs of items that can be referenced later in the test session.
    /// </summary>
    public PropertyBag PropertyBag { get; } = new PropertyBag();

    /// <summary>
    /// Initializes the test session builder with the specified URL and start date.
    /// </summary>
    /// <param name="url">The URL to be used for the test session.</param>
    /// <param name="startDate">The start date of the test session.</param>
    /// <returns>The initialized test session builder.</returns>
    public ITestSessionBuilder Initiate(Uri url, DateTime startDate)
    {
        _startDate = startDate;
        _url = url;

        return this;
    }

    /// <summary>
    /// Builds a test step with the specified component and instrumentation log.
    /// </summary>
    /// <param name="component">The component to be used for the test step.</param>
    /// <param name="instrumentation">The instrumentation log to be used for the test step.</param>
    /// <returns>The built test step.</returns>
    public TestStep Build(ITestComponent component, InstrumentationLog instrumentation)
    {
        ArgumentNullException.ThrowIfNull(component, nameof(component));
        ArgumentNullException.ThrowIfNull(instrumentation, nameof(instrumentation));

        var testStep = new TestStep(
            Name: component.Name,
            StartDate: instrumentation.StartTime,
            Duration: instrumentation.ElapsedTime,
            Type: component.Type,
            Result: TestStepResult.Succeeded,
            ErrorMessage: null);
        _steps.Add(testStep);

        return testStep;
    }

    /// <summary>
    /// Builds a test step with the specified component, instrumentation log, and error.
    /// </summary>
    /// <param name="component">The component to be used for the test step.</param>
    /// <param name="instrumentation">The instrumentation log to be used for the test step.</param>
    /// <param name="error">The error to be used for the test step.</param>
    /// <returns>The built test step.</returns>
    public TestStep Build(ITestComponent component, InstrumentationLog instrumentation, Error error)
    {
        ArgumentNullException.ThrowIfNull(component, nameof(component));
        ArgumentNullException.ThrowIfNull(instrumentation, nameof(instrumentation));
        ArgumentNullException.ThrowIfNull(error, nameof(error));

        var testStep = new TestStep(
            Name: component.Name,
            StartDate: instrumentation.StartTime,
            Duration: instrumentation.ElapsedTime,
            Type: component.Type,
            Result: TestStepResult.Failed,
            ErrorMessage: error);
        _steps.Add(testStep);

        return testStep;
    }

    /// <summary>
    /// Builds a test step with the specified component, instrumentation log, and exception.
    /// </summary>
    /// <param name="component">The component to be used for the test step.</param>
    /// <param name="instrumentation">The instrumentation log to be used for the test step.</param>
    /// <param name="exception">The exception to be used for the test step.</param>
    /// <returns>The built test step.</returns>
    public TestStep Build(ITestComponent component, InstrumentationLog instrumentation, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(component, nameof(component));
        ArgumentNullException.ThrowIfNull(instrumentation, nameof(instrumentation));
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        var testStep = new TestStep(
            Name: component.Name,
            StartDate: instrumentation.StartTime,
            Duration: instrumentation.ElapsedTime,
            Type: component.Type,
            Result: TestStepResult.Failed,
            ErrorMessage: Errors.ExceptionError(exception));
        _steps.Add(testStep);

        return testStep;
    }

    /// <summary>
    /// Gets the test session.
    /// </summary>
    /// <returns>The test session.</returns>
    public TestSession GetTestSession()
    {
        try
        {
            var testSession = new TestSession(_url!, _startDate, _steps, PropertyBag);
            // Set the test session status to complete to indicate that no further modifications are allowed.
            testSession.Complete();

            return testSession;
        }
        catch (Exception ex)
        {
            return TestSession.GetDeclinedTestSession(ex.Message);
        }
    }
}
