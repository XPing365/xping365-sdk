using Microsoft.Extensions.DependencyInjection;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.DependencyInjection;
using XPing365.Sdk.Core.Clients.Browser;
using XPing365.Sdk.Core.Clients.Configurations;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.UnitTests.DependencyInjection;

internal class DependencyInjectionTests
{
    [Test]
    public void AddHttpClientsRegistersIHttpClientFactory()
    {
        // Arrange
        IServiceCollection serviceDescriptors = new ServiceCollection();

        // Act
        serviceDescriptors.AddHttpClientFactory();

        // Assert
        Assert.That(serviceDescriptors.Any(d => d.ServiceType.Name == nameof(IHttpClientFactory)), Is.True);
    }

    [Test]
    public void AddHttpClientsCallsConfigurationActionWhenProvided()
    {
        // Arrange
        IServiceCollection serviceDescriptors = new ServiceCollection();

        bool configurationCalled = false;

        void Configure(IServiceProvider provider, HttpClientFactoryConfiguration config)
        {
            Assert.Multiple(() =>
            {
                Assert.That(provider, Is.Not.Null);
                Assert.That(config, Is.Not.Null);
            });
            configurationCalled = true;
        }

        // Act
        serviceDescriptors.AddHttpClientFactory(Configure);

        // Assert
        Assert.That(configurationCalled, Is.True);
    }

    [Test]
    public void AddBrowserClientsRegistersIBrowserClientFactory()
    {
        // Arrange
        IServiceCollection serviceDescriptors = new ServiceCollection();

        // Act
        serviceDescriptors.AddBrowserClientFactory();

        // Assert
        Assert.That(serviceDescriptors.Any(d => d.ServiceType.Name == nameof(IBrowserFactory)), Is.True);
    }

    [Test]
    public void AddTestAgentRegistersITestSessionBuilder()
    {
        // Arrange
        IServiceCollection serviceDescriptors = new ServiceCollection();

        // Act
        serviceDescriptors.AddTestAgent();

        // Assert
        Assert.That(serviceDescriptors.Any(d => d.ServiceType.Name == nameof(ITestSessionBuilder)), Is.True);
    }

    [Test]
    public void AddTestAgentWithTestAgentBuilderRegistersITestSessionBuilder()
    {
        // Arrange
        IServiceCollection serviceDescriptors = new ServiceCollection();

        // Act
        serviceDescriptors.AddTestAgent((testAgent) => { });

        // Assert
        Assert.That(serviceDescriptors.Any(d => d.ServiceType.Name == nameof(ITestSessionBuilder)), Is.True);
    }

    [Test]
    public void AddNamedTestAgentRegistersITestSessionBuilder()
    {
        // Arrange
        IServiceCollection serviceDescriptors = new ServiceCollection();

        // Act
        serviceDescriptors.AddTestAgent("named test agent", (testAgent) => { });

        // Assert
        Assert.That(serviceDescriptors.Any(d => d.ServiceType.Name == nameof(ITestSessionBuilder)), Is.True);
    }

    [Test]
    public void AddNamedTestAgentCallsTestAgentBuilder()
    {
        // Arrange
        const string testAgentName = "named test agent";

        IServiceCollection serviceDescriptors = new ServiceCollection();

        bool builderCalled = false;

        void Builder(TestAgent agent)
        {
            Assert.That(agent, Is.Not.Null);
            builderCalled = true;
        }

        // Act
        serviceDescriptors.AddTestAgent(testAgentName, Builder);
        var provider = serviceDescriptors.BuildServiceProvider();

        provider.GetKeyedService<TestAgent>(serviceKey: testAgentName);

        // Assert
        Assert.That(builderCalled, Is.True);
    }
}
