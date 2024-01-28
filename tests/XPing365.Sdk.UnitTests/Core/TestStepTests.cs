using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.UnitTests.Core;

public sealed class TestStepTests
{
    [Test]
    public void ThrowsArgumentExceptionWhenStartDateIsInThePast()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new TestStep(
            Name: "TestStepName",
            StartDate: DateTime.UtcNow - TimeSpan.FromDays(2),
            Duration: TimeSpan.Zero,
            Type: TestStepType.ActionStep,
            Result: TestStepResult.Succeeded));
    }

    [Test]
    public void DoesNotThrowWhenStartDateIsNotInThePast()
    {
        // Assert
        Assert.DoesNotThrow(() => new TestStep(
            Name: "TestStepName",
            StartDate: DateTime.Today,
            Duration: TimeSpan.Zero,
            Type: TestStepType.ActionStep,
            Result: TestStepResult.Succeeded));
    }

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
            ErrorMessage: "err msg"));
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
            ErrorMessage: "err msg"));
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
            ErrorMessage: "error message");

        // Assert
        Assert.That(testStep.ErrorMessage, Is.Null);
    }
}
