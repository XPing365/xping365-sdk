﻿using XPing365.Sdk.Core.Session.Comparison;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Session.Comparison.Comparers;

namespace XPing365.Sdk.Core.UnitTests.Session.Comparison;

public sealed class ExecutionDurationComparerTests : ComparerBaseTests<ExecutionDurationComparer>
{
    [Test]
    public void CompareShouldReturnDiffResultWhenExecutionDurationIsEqual()
    {
        // Arrange
        IList<TestStep> steps = [
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
        ];

        TestSession session1 = CreateTestSessionMock(steps: steps);
        TestSession session2 = CreateTestSessionMock(steps: steps);

        // Act
        ExecutionDurationComparer comparer = CreateComparer();
        DiffResult result = comparer.Compare(session1, session2);

        // Assert
        Assert.That(result, Is.EqualTo(DiffResult.Empty));
    }

    [Test]
    public void CompareShouldReturnDiffResultWhenExecutionDurationIsNotEqual()
    {
        // Arrange
        IList<TestStep> steps1 = [
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
        ];

        IList<TestStep> steps2 = [
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(125)),
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(125)),
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(125)),
        ];

        TestSession session1 = CreateTestSessionMock(steps: steps1);
        TestSession session2 = CreateTestSessionMock(steps: steps2);

        // Act
        ExecutionDurationComparer comparer = CreateComparer();
        DiffResult result = comparer.Compare(session1, session2);

        // Assert
        Assert.That(result.Differences, Has.Count.EqualTo(1));
        Assert.That(result.Differences.First().Type, Is.EqualTo(DifferenceType.Changed));
        Assert.That(result.Differences.First().Value1, Is.EqualTo(session1.Duration));
        Assert.That(result.Differences.First().Value2, Is.EqualTo(session2.Duration));
    }

    [Test]
    public void CompareShouldReturnDiffResultWhenDeclinedReasonIsAbsentInFirstButPresentInSecond()
    {
        // Arrange
        IList<TestStep> steps = [
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
        ];

        TestSession session1 = CreateTestSessionMock(steps: null);
        TestSession session2 = CreateTestSessionMock(steps: steps);

        // Act
        ExecutionDurationComparer comparer = CreateComparer();
        DiffResult result = comparer.Compare(session1, session2);

        // Assert
        Assert.That(result.Differences, Has.Count.EqualTo(1));
        Assert.That(result.Differences.First().Type, Is.EqualTo(DifferenceType.Changed));
        Assert.That(result.Differences.First().Value1, Is.EqualTo(TimeSpan.Zero));
        Assert.That(result.Differences.First().Value2, Is.EqualTo(session2.Duration));
    }

    [Test]
    public void CompareShouldReturnDiffResultWhenDeclinedReasonIsAbsentInSecondButPresentInFirst()
    {
        // Arrange
        IList<TestStep> steps = [
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
            CreateTestStepMock(duration: TimeSpan.FromMilliseconds(100)),
        ];

        TestSession session1 = CreateTestSessionMock(steps: steps);
        TestSession session2 = CreateTestSessionMock(steps: null);

        // Act
        ExecutionDurationComparer comparer = CreateComparer();
        DiffResult result = comparer.Compare(session1, session2);

        // Assert
        Assert.That(result.Differences, Has.Count.EqualTo(1));
        Assert.That(result.Differences.First().Type, Is.EqualTo(DifferenceType.Changed));
        Assert.That(result.Differences.First().Value1, Is.EqualTo(session1.Duration));
        Assert.That(result.Differences.First().Value2, Is.EqualTo(TimeSpan.Zero));
    }
}
