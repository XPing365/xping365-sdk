using Moq;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Validators;

namespace XPing365.Sdk.UnitTests;

public sealed class TestAgentTests
{
    private sealed class Agent(params TestStepHandler[] handlers) : TestAgent(handlers)
    { }

    private readonly Mock<TestStepHandler> _testStepHandlerMock = new("StepName", TestStepType.ActionStep);
    private readonly Mock<IProgress<TestStep>> _progressMock = new();
    private readonly Mock<IValidator> _validatorMock = new();

    [SetUp]
    public void Setup()
    {
        _testStepHandlerMock.Reset();
    }

    [Test]
    public void HandlersAreNotNullWhenNoTestStepHandlersAreProvided()
    {
        // Arrange
        var testAgent = new Agent();

        // Assert
        Assert.That(testAgent.Handlers, Is.Not.Null);
    }

    [Test]
    public void HandlersAreEmptyWhenNoTestStepHandlersAreProvided()
    {
        // Arrange
        var testAgent = new Agent();

        // Assert
        Assert.That(testAgent.Handlers, Is.Empty);
    }

    [Test]
    public async Task RunAsyncReturnsTestSessionWhenNoTestStepHandlersAreProvided()
    {
        // Arrange
        var testAgent = new Agent();

        // Act
        TestSession testSession = await testAgent.RunAsync(
            new Uri("http://test"),
            TestSettings.DefaultForAvailability,
            validator: null,
            progress: null).ConfigureAwait(false);

        // Assert
        Assert.That(testSession, Is.Not.Null);
    }

    [Test]
    public async Task RunAsyncReturnsDeclinedTestSessionWhenNoTestStepHandlersAreProvided()
    {
        // Arrange
        var testAgent = new Agent();

        // Act
        TestSession testSession = await testAgent.RunAsync(
            new Uri("http://test"),
            TestSettings.DefaultForAvailability,
            validator: null,
            progress: null).ConfigureAwait(false);

        // Assert
        Assert.That(testSession.State, Is.EqualTo(TestSessionState.Declined));
    }

    [Test]
    public async Task RunAsyncReturnsNotValidTestSessionWhenNoTestStepHandlersAreProvided()
    {
        // Arrange
        var testAgent = new Agent();

        // Act
        TestSession testSession = await testAgent.RunAsync(
            new Uri("http://test"),
            TestSettings.DefaultForAvailability,
            validator: null,
            progress: null).ConfigureAwait(false);

        // Assert
        Assert.That(testSession.IsValid, Is.False);
    }

    [Test]
    public void RunAsyncThrowsArgumentNullExceptionWhenUrlIsNull()
    {
        // Arrange
        var testAgent = new Agent();

        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await testAgent.RunAsync(
            url: null!,
            TestSettings.DefaultForAvailability,
            validator: null,
            progress: null).ConfigureAwait(false));
    }

    [Test]
    public void RunAsyncThrowsArgumentNullExceptionWhenTestSettingsIsNull()
    {
        // Arrange
        var testAgent = new Agent();

        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: null!,
            validator: null,
            progress: null).ConfigureAwait(false));
    }

    [Test]
    public async Task RunAsyncDoesNotReportProgressWhenNoTestStepHandlersAreProvided()
    {
        // Arrange
        var testAgent = new Agent();

        // Act
        TestSession testSession = await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: TestSettings.DefaultForAvailability,
            validator: null,
            progress: _progressMock.Object).ConfigureAwait(false);

        // Assert
        _progressMock.Verify(mock => mock.Report(It.IsAny<TestStep>()), Times.Never);
    }

    [Test]
    public async Task RunAsyncDoesNotReportProgressWhenNoTestStepInstanceIsReturnedFromTestStepHandler()
    {
        // Arrange
        _testStepHandlerMock.Setup(mock => mock.HandleStepAsync(
            It.IsAny<Uri>(),
            It.IsAny<TestSettings>(),
            It.IsAny<TestSession>(),
            It.IsAny<CancellationToken>())).Returns(Task.FromResult<TestStep>(result: null!));
        var testAgent = new Agent(_testStepHandlerMock.Object);

        // Act
        TestSession testSession = await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: TestSettings.DefaultForAvailability,
            validator: null,
            progress: _progressMock.Object).ConfigureAwait(false);

        // Assert
        _progressMock.Verify(mock => mock.Report(It.IsAny<TestStep>()), Times.Never);
    }

    [Test]
    public async Task RunAsyncReportsProgressWhenTestStepInstanceIsReturnedFromTestStepHandler()
    {
        // Arrange
        _testStepHandlerMock.Setup(mock => mock.HandleStepAsync(
            It.IsAny<Uri>(),
            It.IsAny<TestSettings>(),
            It.IsAny<TestSession>(),
            It.IsAny<CancellationToken>())).Returns(Task.FromResult(result: new TestStep(
                Name: "TestStepName",
                StartDate: DateTime.UtcNow,
                Duration: TimeSpan.Zero,
                Type: TestStepType.ActionStep,
                Result: TestStepResult.Succeeded,
                PropertyBag: new PropertyBag(),
                ErrorMessage: null)));
        var testAgent = new Agent(_testStepHandlerMock.Object);

        // Act
        TestSession testSession = await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: TestSettings.DefaultForAvailability,
            validator: null,
            progress: _progressMock.Object).ConfigureAwait(false);

        // Assert
        _progressMock.Verify(mock => mock.Report(It.IsAny<TestStep>()), Times.Once);
    }

    [Test]
    public async Task RunAsyncDoesNotInvokeValidatorWhenNotNullAndNoTestStepHandlersAreProvided()
    {
        // Arrange
        var testAgent = new Agent();

        // Act
        TestSession testSession = await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: TestSettings.DefaultForAvailability,
            validator: _validatorMock.Object,
            progress: null).ConfigureAwait(false);

        // Assert
        _validatorMock.Verify(mock => mock.ValidateAsync(
            It.IsAny<Uri>(),
            It.IsAny<TestSettings>(),
            It.IsAny<TestSession>(),
            null,
            new CancellationToken()), Times.Never);
    }

    [Test]
    public async Task RunAsyncInvokesValidatorWhenNotNullAndTestStepHandlerIsProvided()
    {
        // Arrange
        var testAgent = new Agent(_testStepHandlerMock.Object);

        // Act
        TestSession testSession = await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: TestSettings.DefaultForAvailability,
            validator: _validatorMock.Object,
            progress: null).ConfigureAwait(false);

        // Assert
        _validatorMock.Verify(mock => mock.ValidateAsync(
            It.IsAny<Uri>(),
            It.IsAny<TestSettings>(),
            It.IsAny<TestSession>(),
            null,
            new CancellationToken()), Times.Once);
    }

    [Test]
    public async Task RunAsyncInvokesHandleStepAsyncWhenTestStepHandlerIsProvided()
    {
        // Arrange
        var testAgent = new Agent(_testStepHandlerMock.Object);

        // Act
        TestSession testSession = await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: TestSettings.DefaultForAvailability,
            validator: null,
            progress: null).ConfigureAwait(false);

        // Assert
        _testStepHandlerMock.Verify(mock => mock.HandleStepAsync(
            It.IsAny<Uri>(),
            It.IsAny<TestSettings>(),
            It.IsAny<TestSession>(),
            new CancellationToken()), Times.Once);
    }

    [Test]
    public async Task RunAsyncReturnsTestSessionWithNoTestStepsWhenNoTestStepInstanceIsReturnedFromTestStepHandler()
    {
        // Arrange
        _testStepHandlerMock.Setup(mock => mock.HandleStepAsync(
            It.IsAny<Uri>(),
            It.IsAny<TestSettings>(),
            It.IsAny<TestSession>(),
            It.IsAny<CancellationToken>())).Returns(Task.FromResult<TestStep>(result: null!));
        var testAgent = new Agent(_testStepHandlerMock.Object);

        // Act
        TestSession testSession = await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: TestSettings.DefaultForAvailability,
            validator: null,
            progress: null).ConfigureAwait(false);

        // Assert
        Assert.That(testSession.Steps, Is.Empty);
    }

    [Test]
    public async Task RunAsyncReturnsTestSessionWithTestStepWhenTestStepInstanceIsReturnedFromTestStepHandler()
    {
        // Arrange
        const int expectedTestStepCount = 1;
        _testStepHandlerMock.Setup(mock => mock.HandleStepAsync(
            It.IsAny<Uri>(),
            It.IsAny<TestSettings>(),
            It.IsAny<TestSession>(),
            It.IsAny<CancellationToken>())).Returns(Task.FromResult(result: new TestStep(
                Name: "TestStepName",
                StartDate: DateTime.UtcNow,
                Duration: TimeSpan.Zero,
                Type: TestStepType.ActionStep,
                Result: TestStepResult.Succeeded,
                PropertyBag: new PropertyBag(),
                ErrorMessage: null)));
        var testAgent = new Agent(_testStepHandlerMock.Object);

        // Act
        TestSession testSession = await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: TestSettings.DefaultForAvailability,
            validator: null,
            progress: null).ConfigureAwait(false);

        // Assert
        Assert.That(testSession.Steps, Has.Count.EqualTo(expectedTestStepCount));
    }

    [Test]
    public async Task RunAsyncReturnsTestSessionWithFailuresWhenFailedTestStepInstanceIsReturnedFromTestStepHandler()
    {
        // Arrange
        const int expectedFailuresCount = 1;
        _testStepHandlerMock.Setup(mock => mock.HandleStepAsync(
            It.IsAny<Uri>(),
            It.IsAny<TestSettings>(),
            It.IsAny<TestSession>(),
            It.IsAny<CancellationToken>())).Returns(Task.FromResult(result: new TestStep(
                Name: "TestStepName",
                StartDate: DateTime.UtcNow,
                Duration: TimeSpan.Zero,
                Type: TestStepType.ActionStep,
                Result: TestStepResult.Failed,
                PropertyBag: new PropertyBag(),
                ErrorMessage: "ErrorMessage")));
        var testAgent = new Agent(_testStepHandlerMock.Object);

        // Act
        TestSession testSession = await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: TestSettings.DefaultForAvailability,
            validator: null,
            progress: null).ConfigureAwait(false);

        // Assert
        Assert.That(testSession.Failures, Has.Count.EqualTo(expectedFailuresCount));
    }
}
