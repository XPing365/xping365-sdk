using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.UnitTests.Components;

public sealed class TestSettignsTests
{
    [Test]
    public void ClassTypeShouldBeSealed()
    {
        Type type = typeof(TestSettings);
        Assert.That(type.IsSealed, Is.True, $"The class '{nameof(TestSettings)}' should be sealed.");
    }

    [Test]
    public void ContinueOnFailureShouldBeDisabledByDefault()
    {
        // Arrange
        var settings = new TestSettings();

        // Assert
        Assert.That(settings.ContinueOnFailure, Is.False);
    }

    [Test]
    public void TimeoutShouldBeSetTo30SecondsByDefault()
    {
        // Arrange
        var settings = new TestSettings();

        // Assert
        Assert.That(settings.Timeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
    }
}
