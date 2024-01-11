using XPing365.Sdk.Core;

namespace XPing365.Sdk.UnitTests;

public sealed class TestStepHandlerTests
{
    private sealed class Handler(string name, TestStepType type) : TestStepHandler(name, type)
    {
        public override Task<TestStep> HandleStepAsync(
            Uri uri,
            TestSettings settings,
            TestSession session,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TestStep(
                Name: null!,
                StartDate: DateTime.UtcNow,
                Duration: TimeSpan.Zero,
                Type: TestStepType.ActionStep,
                Result: TestStepResult.Succeeded,
                PropertyBag: new PropertyBag(),
                "err msg"));
        }
    }

    [Test]
    public void ThrowsArgumentExceptionWhenNameIsNullOrEmpty()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new Handler(name: null!, type: TestStepType.ActionStep));
    }
}
