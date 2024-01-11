using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core;

public abstract class TestStepHandler(string name, TestStepType type)
{
    public string Name { get; } = name.RequireNotNullOrEmpty(nameof(name));
    public TestStepType Type { get; } = type;

    public abstract Task<TestStep> HandleStepAsync(
        Uri uri,
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
