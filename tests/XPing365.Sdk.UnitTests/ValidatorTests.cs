using Moq;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Validators;

namespace XPing365.Sdk.UnitTests;

public sealed class ValidatorTests
{
    private readonly Mock<TestStepHandler> _testStepHandlerMock = new("StepName", TestStepType.ValidateStep);
    private readonly Mock<IProgress<TestStep>> _progressMock = new();

    [Test]
    public void ValidatorsAreEmptyWhenNewlyInstantiatedWithNoTestStepHandlers()
    {
        // Arrange
        var validator = new Validator();

        // Assert
        Assert.That(validator.Validators, Is.Empty);
    }

    [Test]
    public void ValidateThrowsArgumentNullExceptionWhenUriIsNull()
    {
        // Arrange 
        var validator = new Validator();

        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await validator.ValidateAsync(
                null!, 
                TestSettings.DefaultForAvailability, 
                new TestSession(DateTime.UtcNow, new Uri("http://test")), 
                null, new CancellationToken())
            .ConfigureAwait(false));
    }

    [Test]
    public void ValidateThrowsArgumentNullExceptionWhenTestSettingsIsNull()
    {
        // Arrange 
        var validator = new Validator();

        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await validator.ValidateAsync(
                new Uri("http://test"),
                null!,
                new TestSession(DateTime.UtcNow, new Uri("http://test")),
                null, new CancellationToken())
            .ConfigureAwait(false));
    }

    [Test]
    public void ValidateThrowsArgumentNullExceptionWhenTestSessionIsNull()
    {
        // Arrange 
        var validator = new Validator();

        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await validator.ValidateAsync(
                new Uri("http://test"),
                TestSettings.DefaultForAvailability,
                null!,
                null, new CancellationToken())
            .ConfigureAwait(false));
    }

    [Test]
    public async Task ValidateReportsProgressWhenTestStepIsReturnedFromTestStepHandler()
    {
        // Arrange
        _testStepHandlerMock.Setup(mock => mock.HandleStepAsync(
                It.IsAny<Uri>(),
                It.IsAny<TestSettings>(),
                It.IsAny<TestSession>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(result: new TestStep(
                Name: "TestStepName",
                StartDate: DateTime.UtcNow,
                Duration: TimeSpan.Zero,
                Type: TestStepType.ActionStep,
                Result: TestStepResult.Succeeded,
                PropertyBag: new PropertyBag(),
                ErrorMessage: null)));
        var validator = new Validator(_testStepHandlerMock.Object);
        var urlUnderTest = new Uri("http://test");

        // Act
        await validator.ValidateAsync(
                url: urlUnderTest,
                settings: TestSettings.DefaultForAvailability,
                session: new TestSession(
                    startDate: DateTime.UtcNow, 
                    url: urlUnderTest), 
                progress: _progressMock.Object)
            .ConfigureAwait(false);

        // Assert
        _progressMock.Verify(mock => mock.Report(It.IsAny<TestStep>()), Times.Once);
    }

    [Test]
    public async Task ValidateDoesNotReportProgressWhenTestStepIsNotReturnedFromTestStepHandler()
    {
        // Arrange
        _testStepHandlerMock.Setup(mock => mock.HandleStepAsync(
                It.IsAny<Uri>(),
                It.IsAny<TestSettings>(),
                It.IsAny<TestSession>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<TestStep>(result: null!));
        var validator = new Validator(_testStepHandlerMock.Object);
        var urlUnderTest = new Uri("http://test");

        // Act
        await validator.ValidateAsync(
                url: urlUnderTest,
                settings: TestSettings.DefaultForAvailability,
                session: new TestSession(
                    startDate: DateTime.UtcNow,
                    url: urlUnderTest),
                progress: _progressMock.Object)
            .ConfigureAwait(false);

        // Assert
        _progressMock.Verify(mock => mock.Report(It.IsAny<TestStep>()), Times.Never);
    }

    [Test]
    public async Task TestSessionIncludesTestStepWhenTestStepIsReturnedFromTestStepHandler()
    {
        // Arrange
        const int expectedTestStepsCount = 1;
        _testStepHandlerMock.Setup(mock => mock.HandleStepAsync(
                It.IsAny<Uri>(),
                It.IsAny<TestSettings>(),
                It.IsAny<TestSession>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(result: new TestStep(
                Name: "TestStepName",
                StartDate: DateTime.UtcNow,
                Duration: TimeSpan.Zero,
                Type: TestStepType.ActionStep,
                Result: TestStepResult.Succeeded,
                PropertyBag: new PropertyBag(),
                ErrorMessage: null)));
        var validator = new Validator(_testStepHandlerMock.Object);
        var urlUnderTest = new Uri("http://test");
        var testSession = new TestSession(
            startDate: DateTime.UtcNow,
            url: urlUnderTest);
        
        // Act
        await validator.ValidateAsync(
                url: urlUnderTest,
                settings: TestSettings.DefaultForAvailability,
                session: testSession,
                progress: null)
            .ConfigureAwait(false);

        // Assert
        Assert.That(testSession.Steps, Has.Count.EqualTo(expectedTestStepsCount));
    }

    [Test]
    public async Task TestSessionDoesNotIncludeTestStepWhenTestStepIsNotReturnedFromTestStepHandler()
    {
        // Arrange
        const int expectedTestStepsCount = 0;
        _testStepHandlerMock.Setup(mock => mock.HandleStepAsync(
                It.IsAny<Uri>(),
                It.IsAny<TestSettings>(),
                It.IsAny<TestSession>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<TestStep>(result: null!));
        var validator = new Validator(_testStepHandlerMock.Object);
        var urlUnderTest = new Uri("http://test");
        var testSession = new TestSession(
            startDate: DateTime.UtcNow,
            url: urlUnderTest);

        // Act
        await validator.ValidateAsync(
                url: urlUnderTest,
                settings: TestSettings.DefaultForAvailability,
                session: testSession,
                progress: null)
            .ConfigureAwait(false);

        // Assert
        Assert.That(testSession.Steps, Has.Count.EqualTo(expectedTestStepsCount));
    }
}
