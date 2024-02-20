namespace XPing365.Sdk.UnitTests.Components;

using Moq;
using XPing365.Sdk.Core.Session;
using TestContext = Sdk.Core.Components.TestContext;

internal class TestContextTests
{
    [Test]
    public void ConstructorThrowsArgumentNullExecptionWhenISessionBuilderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TestContext(sessionBuilder: null!, progress: null));
    }

    [Test]
    public void ConstructorDoesNotThrowWhenISessionBuilderIsNotNull()
    {
        Assert.DoesNotThrow(() => new TestContext(sessionBuilder: Mock.Of<ITestSessionBuilder>(), progress: null));
    }

    [Test]
    public void ConstructorDoesNotThrowWhenIProgressIsNull()
    {
        Assert.DoesNotThrow(() => new TestContext(sessionBuilder: Mock.Of<ITestSessionBuilder>(), progress: null));
    }

    [Test]
    public void ConstructorDoesNotThrowWhenIProgressIsNotNull()
    {
        Assert.DoesNotThrow(() => new TestContext(
            sessionBuilder: Mock.Of<ITestSessionBuilder>(), 
            progress: Mock.Of<IProgress<TestStep>>()));
    }
}
