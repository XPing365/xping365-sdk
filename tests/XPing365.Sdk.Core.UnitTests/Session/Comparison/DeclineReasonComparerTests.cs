﻿using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Session.Comparison;
using XPing365.Sdk.Core.Session.Comparison.Comparers;

namespace XPing365.Sdk.Core.UnitTests.Session.Comparison;

public sealed class DeclineReasonComparerTests : ComparerBaseTests<DeclineReasonComparer>
{
    [Test]
    public void CompareShouldReturnDiffResultWhenDeclinedReasonIsEqual()
    {
        // Arrange
        const string declineReason = "DeclinedText";

        TestSession session1 = CreateTestSessionMock(declineReason: declineReason);
        TestSession session2 = CreateTestSessionMock(declineReason: declineReason);

        // Act
        DeclineReasonComparer comparer = CreateComparer();
        DiffResult result = comparer.Compare(session1, session2);

        // Assert
        Assert.That(result, Is.EqualTo(DiffResult.Empty));
    }

    [Test]
    public void CompareShouldReturnDiffResultWhenDeclinedReasonIsNotEqual()
    {
        // Arrange
        const string declineReason1 = "DeclinedText1";
        const string declineReason2 = "DeclinedText2";

        TestSession session1 = CreateTestSessionMock(declineReason: declineReason1);
        TestSession session2 = CreateTestSessionMock(declineReason: declineReason2);

        // Act
        DeclineReasonComparer comparer = CreateComparer();
        DiffResult result = comparer.Compare(session1, session2);

        // Assert
        Assert.That(result.Differences, Has.Count.EqualTo(1));
        Assert.That(result.Differences.First().Type, Is.EqualTo(DifferenceType.Changed));
        Assert.That(result.Differences.First().Value1, Is.EqualTo(declineReason1));
        Assert.That(result.Differences.First().Value2, Is.EqualTo(declineReason2));
    }

    [Test]
    public void CompareShouldReturnDiffResultWhenDeclinedReasonIsAbsentInFirstButPresentInSecond()
    {
        // Arrange
        const string declineReason = "DeclinedText";

        TestSession session1 = CreateTestSessionMock(declineReason: null);
        TestSession session2 = CreateTestSessionMock(declineReason: declineReason);

        // Act
        DeclineReasonComparer comparer = CreateComparer();
        DiffResult result = comparer.Compare(session1, session2);

        // Assert
        Assert.That(result.Differences, Has.Count.EqualTo(1));
        Assert.That(result.Differences.First().Type, Is.EqualTo(DifferenceType.Added));
        Assert.That(result.Differences.First().Value1, Is.Null);
        Assert.That(result.Differences.First().Value2, Is.EqualTo(declineReason));
    }

    [Test]
    public void CompareShouldReturnDiffResultWhenDeclinedReasonIsAbsentInSecondButPresentInFirst()
    {
        // Arrange
        const string declineReason = "DeclinedText";

        TestSession session1 = CreateTestSessionMock(declineReason: declineReason);
        TestSession session2 = CreateTestSessionMock(declineReason: null);

        // Act
        DeclineReasonComparer comparer = CreateComparer();
        DiffResult result = comparer.Compare(session1, session2);

        // Assert
        Assert.That(result.Differences, Has.Count.EqualTo(1));
        Assert.That(result.Differences.First().Type, Is.EqualTo(DifferenceType.Removed));
        Assert.That(result.Differences.First().Value1, Is.EqualTo(declineReason));
        Assert.That(result.Differences.First().Value2, Is.Null);
    }
}
