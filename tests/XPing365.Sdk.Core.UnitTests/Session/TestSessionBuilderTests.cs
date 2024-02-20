﻿using System;
using Moq;
using Polly;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.UnitTests.Session;

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
    public void HasFailedReturnsTrueWhenFailingStepHasBeenAdded()
    {
        // Arrange
        const bool expectedResult = true;

        var builder = new TestSessionBuilder();
        using var instrumentation = new InstrumentationLog();
        var mockedComponent = new Mock<ITestComponent>();
        mockedComponent.SetupGet(c => c.Name).Returns("ComponenName");
        mockedComponent.SetupGet(c => c.Type).Returns(TestStepType.ActionStep);

        // Act
        builder.Build(mockedComponent.Object, instrumentation, new Error("code", "message"));

        // Assert
        Assert.That(builder.HasFailed, Is.EqualTo(expectedResult));
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