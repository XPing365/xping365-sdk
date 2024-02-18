using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.UnitTests.Session;

public sealed class TestSessionTests
{
    private static TestSession CreateTestSessionUnderTest(
        Uri? url = null,
        DateTime? startDate = null,
        ICollection<TestStep>? steps = null) => new()
        {
            Url = url ?? new Uri("https://test"),
            StartDate = startDate ?? DateTime.UtcNow,
            Steps = steps?.ToList().AsReadOnly() ?? new List<TestStep>().AsReadOnly(),
            State = TestSessionState.NotStarted
        };

    private static TestStep CreateTestStepMock(
        string name = "TestStepMock",
        DateTime? startDate = null,
        TimeSpan? duration = null,
        TestStepType type = TestStepType.ActionStep,
        TestStepResult result = TestStepResult.Succeeded,
        string? errorMessage = null) => new()
        {
            Name = name,
            StartDate = startDate ?? DateTime.UtcNow,
            Duration = duration ?? TimeSpan.Zero,
            Type = type,
            Result = result,
            ErrorMessage = errorMessage,
            PropertyBag = null
        };

    [Test]
    public void ThrowsArgumentNullExceptionWhenInstantiatedWithNullUri()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new TestSession
        {
            Url = null!,
            StartDate = DateTime.UtcNow,
            Steps = [],
            State = TestSessionState.NotStarted
        });
    }

    [Test]
    public void NotStartedWhenNewlyInstantiated()
    {
        // Arrange
        var testSession = CreateTestSessionUnderTest();

        // Assert
        Assert.That(testSession.State, Is.EqualTo(TestSessionState.NotStarted));
    }

    [Test]
    public void HasZeroStepsWhenInstantiatedWithZeroSteps()
    {
        // Arrange
        var testSession = CreateTestSessionUnderTest(steps: []);

        // Assert
        Assert.That(testSession.Steps, Is.Empty);
    }

    [Test]
    public void HasZeroFailuresWhenNoFailuresHasBeenAdded()
    {
        // Arrange
        var testSession = CreateTestSessionUnderTest(steps: []);

        // Assert
        Assert.That(testSession.Failures, Is.Empty);
    }

    [Test]
    public void IsValidWhenNoFailuresHasBeenGiven()
    {
        // Arrange
        var testSession = CreateTestSessionUnderTest(steps: [CreateTestStepMock()]);

        // Assert
        Assert.That(testSession.IsValid, Is.True);
    }

    [Test]
    public void HasFailedTestStepsWhenFailedTestStepHasBeenAdded()
    {
        // Arrange
        const int expectedItemCount = 1;

        // Act
        var testSession = CreateTestSessionUnderTest(
            steps: [CreateTestStepMock(result: TestStepResult.Failed, errorMessage: "error")]);

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

        // Act
        var testSession = CreateTestSessionUnderTest(steps: [CreateTestStepMock()]);

        // Assert
        Assert.That(testSession.Failures, Has.Count.EqualTo(expectedItemCount));
    }

    [Test]
    public void DurationShowsTotalElapsedTimeSpentInAllTestSteps()
    {
        // Arrange
        const int testStepDurationInSeconds = 5;
        const int testStepCount = 5;

        var steps = new List<TestStep>();

        for (int i = 0; i < testStepCount; i++)
        {
            steps.Add(CreateTestStepMock(duration: TimeSpan.FromSeconds(testStepDurationInSeconds)));
        }

        // Act
        var testSession = CreateTestSessionUnderTest(steps: steps);

        // Assert
        Assert.That(testSession.Duration,
            Is.EqualTo(TimeSpan.FromSeconds(testStepCount * testStepDurationInSeconds)));
    }
}
