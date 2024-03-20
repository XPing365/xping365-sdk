﻿using XPing365.Sdk.Core.Session.Comparison;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Session.Comparison.Comparers;

namespace XPing365.Sdk.Core.UnitTests.Session.Comparison;

public sealed class UrlComparerTests : ComparerBaseTests<UrlComparer>
{
    [Test]
    public void CompareShouldReturnDiffResultWhenUrlIsEqual()
    {
        // Arrange
        Uri url = new("http://test.com");

        TestSession session1 = CreateTestSessionMock(url: url);
        TestSession session2 = CreateTestSessionMock(url: url);

        // Act
        UrlComparer comparer = CreateComparer();
        DiffResult result = comparer.Compare(session1, session2);

        // Assert
        Assert.That(result, Is.EqualTo(DiffResult.Empty));
    }

    [Test]
    public void CompareShouldReturnDiffResultWhenUrlIsNotEqual()
    {
        // Arrange
        Uri url1 = new("http://test1.com");
        Uri url2 = new("http://test2.com");

        TestSession session1 = CreateTestSessionMock(url: url1);
        TestSession session2 = CreateTestSessionMock(url: url2);

        // Act
        UrlComparer comparer = CreateComparer();
        DiffResult result = comparer.Compare(session1, session2);

        // Assert
        Assert.That(result.Differences, Has.Count.EqualTo(1));
        Assert.That(result.Differences.First().Type, Is.EqualTo(DifferenceType.Changed));
        Assert.That(result.Differences.First().Value1, Is.EqualTo(session1.Url));
        Assert.That(result.Differences.First().Value2, Is.EqualTo(session2.Url));
    }
}
