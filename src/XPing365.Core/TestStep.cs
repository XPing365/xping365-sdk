namespace XPing365.Core;

public sealed record TestStep(
    string Name,
    DateTime StartDate,
    TimeSpan Duration,
    TestStepType Type,
    TestStepResult Result,
    PropertyBag PropertyBag,
    string? ErrorMessage = null)
{
    public static TestStep CreateActionStepFromException(string name, Exception e, InstrumentationLog instrumentation)
    {
        ArgumentNullException.ThrowIfNull(e, nameof(e));
        ArgumentNullException.ThrowIfNull(instrumentation, nameof(instrumentation));

        var testStep = new TestStep(
            Name: name,
            StartDate: instrumentation.StartTime,
            Duration: instrumentation.ElapsedTime,
            Type: TestStepType.ActionStep,
            Result: TestStepResult.Failed,
            PropertyBag: new PropertyBag(),
            ErrorMessage: e.Message);

        return testStep;
    }
}
