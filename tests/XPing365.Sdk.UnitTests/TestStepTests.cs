using XPing365.Sdk.Core;

namespace XPing365.Sdk.UnitTests;

public sealed class TestStepTests
{
    [Test]
    public void ThrowsArgumentExceptionWhenNameIsNullOrEmpty()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new TestStep(
            Name: null!,
            StartDate: DateTime.UtcNow,
            Duration: TimeSpan.Zero,
            Type: TestStepType.ActionStep,
            Result: TestStepResult.Succeeded,
            PropertyBag: new PropertyBag(),
            "err msg"));
    }

    [Test]
    public void ThrowsArgumentExceptionWhenPropertyBagIsNull()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new TestStep(
            Name: null!,
            StartDate: DateTime.UtcNow,
            Duration: TimeSpan.Zero,
            Type: TestStepType.ActionStep,
            Result: TestStepResult.Succeeded,
            PropertyBag: null!,
            "err msg"));
    }

    [Test]
    public void ThrowsArgumentExceptionWhenResulIsFailedAndErrorMessageIsNullOrEmpty()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new TestStep(
            Name: null!,
            StartDate: DateTime.UtcNow,
            Duration: TimeSpan.Zero,
            Type: TestStepType.ActionStep,
            Result: TestStepResult.Failed,
            PropertyBag: null!,
            ErrorMessage: null));
    }

    [Test]
    public void ErrorMessageIsNullWhenResultIsSucceeded()
    {
        // Arrange
        var testStep = new TestStep(
            Name: "TestStepName",
            StartDate: DateTime.UtcNow,
            Duration: TimeSpan.Zero,
            Type: TestStepType.ActionStep,
            Result: TestStepResult.Succeeded,
            PropertyBag: new PropertyBag(),
            ErrorMessage: "error message");

        // Assert
        Assert.That(testStep.ErrorMessage, Is.Null);
    }
}
