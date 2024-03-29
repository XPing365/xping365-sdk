﻿using Moq;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.UnitTests.TestFixtures;

namespace XPing365.Sdk.UnitTests.Core;

[SetUpFixture]
[TestFixtureSource(typeof(TestFixtureProvider), nameof(TestFixtureProvider.ServiceProvider))]
public sealed class TestAgentTests(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly Mock<TestComponent> _testComponentMock = new("StepName", TestStepType.ActionStep);

    [SetUp]
    public void Setup()
    {
        _testComponentMock.Reset();
    }

    [Test]
    public void ContainerIsNotNullWhenComponentIsProvided()
    {
        // Arrange
        var testAgent = new TestAgent(_serviceProvider, Mock.Of<ITestComponent>());

        // Assert
        Assert.That(testAgent.Container, Is.Not.Null);
    }

    [Test]
    public void RunAsyncThrowsArgumentNullExceptionWhenUrlIsNull()
    {
        // Arrange
        var testAgent = new TestAgent(_serviceProvider, Mock.Of<ITestComponent>());

        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await testAgent.RunAsync(
            url: null!,
            TestSettings.Default).ConfigureAwait(false));
    }

    [Test]
    public void RunAsyncThrowsArgumentNullExceptionWhenTestSettingsIsNull()
    {
        // Arrange
        var testAgent = new TestAgent(_serviceProvider, Mock.Of<ITestComponent>());

        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await testAgent.RunAsync(
            url: new Uri("http://test"),
            settings: null!).ConfigureAwait(false));
    }
}
