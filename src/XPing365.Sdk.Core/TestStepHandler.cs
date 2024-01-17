using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core;

/// <summary>
/// This abstract class is used to perform action or validate test operation defined by the derived class and to 
/// create a new instance of a <see cref="TestStep"/> class.
/// </summary>
/// <param name="name">A string representation of the step name.</param>
/// <param name="type">Determines <see cref="TestStepType"/> type of the current instance.</param>
public abstract class TestStepHandler(string name, TestStepType type)
{
    /// <summary>
    /// Gets a step name.
    /// </summary>
    public string Name { get; } = name.RequireNotNullOrEmpty(nameof(name));

    /// <summary>
    /// Gets a step type.
    /// </summary>
    public TestStepType Type { get; } = type;

    /// <summary>
    /// The HandleStepAsync method is used to create <see cref="TestStep"/> object and perform action or validate test 
    /// operation. It is an abstract method that must be implemented by the subclass.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestSession"/> object that represents the test session.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    /// <returns><see cref="TestStep"/> object.</returns>
    public abstract Task<TestStep> HandleStepAsync(
        Uri url,
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default);

    protected TestStep CreateTestStepFromException(
        Exception exception,
        DateTime startTime,
        TimeSpan elapsedTime)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        var testStep = new TestStep(
            Name: Name,
            StartDate: startTime,
            Duration: elapsedTime,
            Type: Type,
            Result: TestStepResult.Failed,
            PropertyBag: new PropertyBag(),
            ErrorMessage: Errors.ExceptionError(exception));

        return testStep;
    }

    protected TestStep CreateFailedTestStep(string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));

        var testStep = new TestStep(
            Name: Name,
            StartDate: DateTime.UtcNow,
            Duration: TimeSpan.Zero,
            Type: Type,
            Result: TestStepResult.Failed,
            PropertyBag: new PropertyBag(),
            ErrorMessage: errorMessage);

        return testStep;
    }

    protected TestStep CreateSuccessTestStep(
        DateTime startTime,
        TimeSpan elapsedTime,
        PropertyBag propertyBag)
    {
        ArgumentNullException.ThrowIfNull(propertyBag, nameof(propertyBag));

        var testStep = new TestStep(
            Name: Name,
            StartDate: startTime,
            Duration: elapsedTime,
            Type: Type,
            Result: TestStepResult.Succeeded,
            PropertyBag: propertyBag,
            ErrorMessage: null);

        return testStep;
    }
}
