using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components.Session;

namespace XPing365.Sdk.UnitTests.Core;

public sealed class TestSessionBuilderTests
{
    [Test]
    public void PropertyBagIsNotNullAfterBuilderCreattion()
    {
        // Act
        var builder = new TestSessionBuilder();

        // Assert
        Assert.That(builder, Is.Not.Null);
    }

    [Test]
    public void HasFailedReturnsFalseAfterBuilderCreation()
    {
        // Act
        var builder = new TestSessionBuilder();

        // Assert
        Assert.That(builder.HasFailed, Is.False);
    }

    [Test]
    public void GetTestSessionReturnsDeclinedSessionWhenBuilderNotInitiated()
    {
        // Act
        var builder = new TestSessionBuilder();

        // Assert
        Assert.That(builder.GetTestSession().State, Is.EqualTo(TestSessionState.Declined));
    }

    [Test]
    public void DeclinedReasonHasSpecificTextWhenUrlIsNullDuringBuilderInitialization()
    {
        // Arrange
        string expectedDeclineReason = Errors.MissingUrlInTestSession;
        var builder = new TestSessionBuilder();

        // Act
        builder.Initiate(url: null!, DateTime.UtcNow);

        // Assert
        Assert.That(builder.GetTestSession().DeclineReason, Does.StartWith(expectedDeclineReason));
    }

    [Test]
    public void DeclinedReasonHasSpecificTextWhenStartDateIsInThePast()
    {
        // Arrange
        string expectedDeclineReason = Errors.IncorrectStartDate;
        var builder = new TestSessionBuilder();

        // Act
        builder.Initiate(url: new Uri("http://test.com"), DateTime.UtcNow - TimeSpan.FromDays(2));

        // Assert
        Assert.That(builder.GetTestSession().DeclineReason, Does.StartWith(expectedDeclineReason));
    }
}
