using XPing365.Sdk.Core;

namespace XPing365.Sdk.UnitTests;

public sealed class TestSessionTests
{
    private static TestSession CreateTestSession() => new(startDate: DateTime.UtcNow, url: new Uri("https://test"));
    private static TestStep CreateTestStep(
        TestStepType type = TestStepType.ActionStep, 
        TestStepResult result = TestStepResult.Succeeded,
        TimeSpan? elapsedTime = null) => new(
            Name: "testStepName",
            StartDate: DateTime.UtcNow,
            Duration: elapsedTime ?? TimeSpan.Zero,
            Type: type,
            Result: result,
            PropertyBag: new PropertyBag(),
            ErrorMessage: "error message");

    [Test]
    public void ThrowsArgumentNullExceptionWhenInstantiatedWithNullUri()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new TestSession(DateTime.UtcNow, url: null!));
    }

    [Test]
    public void NotStartedWhenNewlyInstantiated()
    {
        // Arrange
        var testSession = CreateTestSession();

        // Assert
        Assert.That(testSession.State, Is.EqualTo(TestSessionState.NotStarted));
    }

    [Test]
    public void HasZeroStepsWhenNewlyInstantiated()
    {
        // Arrange
        var testSession = CreateTestSession();

        // Assert
        Assert.That(testSession.Steps, Is.Empty);
    }

    [Test]
    public void HasZeroFailuresWhenNewlyInstantiated()
    {
        // Arrange
        var testSession = CreateTestSession();

        // Assert
        Assert.That(testSession.Failures, Is.Empty);
    }

    [Test]
    public void IsNotValidWhenNewlyInstantiated()
    {
        // Arrange
        var testSession = CreateTestSession();

        // Assert
        Assert.That(testSession.IsValid, Is.False);
    }

    [Test]
    public void IsMarkedCompletedAfterCompleteMethodCalled()
    {
        // Arrange
        var testSession = CreateTestSession();

        // Act
        testSession.Complete();

        // Assert
        Assert.That(testSession.State, Is.EqualTo(TestSessionState.Completed));
    }

    [Test]
    public void IsMarkedDeclinedAfterDeclineMethodCalled()
    {
        // Arrange
        const string declineReason = "declinded";
        var testSession = CreateTestSession();

        // Act
        testSession.Decline(declineReason: declineReason);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(testSession.State, Is.EqualTo(TestSessionState.Declined));
            Assert.That(testSession.DeclineReason, Is.EqualTo(declineReason));
        });
    }

    [Test]
    public void ThrowsExceptionWhenDeclinedAndEmptyDeclineReasonProvided()
    {
        // Arrange
        var testSession = CreateTestSession();

        // Act
        Assert.Throws<ArgumentException>(() => testSession.Decline(declineReason: string.Empty));
    }

    [Test]
    public void HasFailedTestStepsWhenFailedTestStepHasBeenAdded()
    {
        // Arrange
        const int expectedItemCount = 1;
        var testSession = CreateTestSession();
        var testStep = CreateTestStep(result: TestStepResult.Failed);

        // Act
        testSession.AddTestStep(testStep);

        // Assert
        Assert.Multiple(() =>
        {         
            Assert.That(testSession.Steps, Has.Count.EqualTo(expectedItemCount));
            Assert.That(testSession.Failures, Has.Count.EqualTo(expectedItemCount));
        });
    }

    [Test]
    public void HasZeroFailedTestStepsWhenSuccessTestStepHasBeenAdded()
    {
        // Arrange
        const int expectedItemCount = 0;
        var testSession = CreateTestSession();
        var testStep = CreateTestStep(result: TestStepResult.Succeeded);

        // Act
        testSession.AddTestStep(testStep);

        // Assert
        Assert.That(testSession.Failures, Has.Count.EqualTo(expectedItemCount));
    }

    [Test]
    public void DurationShowsTotalElapsedTimeSpentInAllTestSteps()
    {
        // Arrange
        const int testStepDurationInSeconds = 5;
        const int testStepCount = 5;
        var testSession = CreateTestSession();

        // Act
        for (int i = 0; i < testStepCount; i++)
        {
            testSession.AddTestStep(CreateTestStep(elapsedTime: TimeSpan.FromSeconds(testStepDurationInSeconds)));
        }

        // Assert
        Assert.That(testSession.Duration, 
            Is.EqualTo(TimeSpan.FromSeconds(testStepCount * testStepDurationInSeconds)));
    }
}
