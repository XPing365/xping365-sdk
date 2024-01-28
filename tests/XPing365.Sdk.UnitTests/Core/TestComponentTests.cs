using Moq;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.UnitTests.Core;

public sealed class TestComponentTests
{
    private sealed class TestComponentUnderTest(
        string name = nameof(TestComponentUnderTest),
        TestStepType type = TestStepType.ActionStep,
        Mock<ICompositeTests>? compositeMock = null) : TestComponent(name, type)
    {
        public override Task HandleAsync(
            Uri uri,
            TestSettings settings,
            Sdk.Core.Components.TestContext session,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Mock<ICompositeTests>? CompositeMock { get; } = compositeMock;

        internal override ICompositeTests? GetComposite() => CompositeMock?.Object;
    }

    [Test]
    public void ThrowsArgumentExceptionWhenNameIsNullOrEmpty()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new TestComponentUnderTest(name: null!));
        Assert.Throws<ArgumentException>(() => new TestComponentUnderTest(name: string.Empty));
    }

    [Test]
    public void ProbeThrowsArgumentNullExceptionWhenUrlIsNull()
    {
        // Arrange
        TestComponent component = new TestComponentUnderTest();

        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => component.ProbeAsync(url: null!, new TestSettings()));
    }

    [Test]
    public void ProbeThrowsArgumentNullExceptionWhenTestSettingsIsNull()
    {
        // Arrange
        TestComponent component = new TestComponentUnderTest();

        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => component.ProbeAsync(
            url: new Uri("http://test.com"), settings: null!));
    }

    [Test]
    public void AddComponentIsInvokedWhenGetCompositeReturnsObject()
    {
        // Arrange
        var compositeMock = new Mock<ICompositeTests>();
        var component = new TestComponentUnderTest(compositeMock: compositeMock);

        // Act
        component.AddComponent(Mock.Of<ITestComponent>());

        // Assert
        compositeMock.Verify(mock => mock.AddComponent(It.IsAny<ITestComponent>()), Times.Once);
    }

    [Test]
    public void RemoveComponentIsInvokedWhenGetCompositeReturnsObject()
    {
        // Arrange
        var compositeMock = new Mock<ICompositeTests>();
        var component = new TestComponentUnderTest(compositeMock: compositeMock);

        // Act
        component.RemoveComponent(Mock.Of<ITestComponent>());

        // Assert
        compositeMock.Verify(mock => mock.RemoveComponent(It.IsAny<ITestComponent>()), Times.Once);
    }

    [Test]
    public void ComponentsReturnAnEmptyArrayWhenGetCompositeIsNUll()
    {
        // Arrange
        var component = new TestComponentUnderTest(compositeMock: null);

        // Assert
        Assert.That(component.Components, Is.Empty);
    }

    [Test]
    public void ComponentsReturnSpecificArrayWhenGetCompositeIsImplemented()
    {
        // Arrange
        const int expectedChildComponents = 1;
        var compositeMock = new Mock<ICompositeTests>();
        compositeMock.SetupGet(mock => mock.Components).Returns(new[] {Mock.Of<ITestComponent>()});

        // Act
        var component = new TestComponentUnderTest(compositeMock: compositeMock);

        // Assert
        Assert.That(component.Components, Has.Count.EqualTo(expectedChildComponents));
    }
}
