using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XPing365.Sdk.Availability.Browser.DependencyInjection;
using XPing365.Sdk.Availability.DependencyInjection;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.IntegrationTests.TestFixtures;

public static class TestFixtureProvider
{
    private static readonly Lazy<IHost> HostInstance = new(valueFactory: CreateHost, isThreadSafe: true);

    public static IServiceProvider[] ServiceProvider()
    {
        return [HostInstance.Value.Services];
    }

    private static IHost CreateHost()
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(implementationInstance: Mock.Of<IProgress<TestStep>>());
            services.AddHttpClientTestAgent();
            services.AddBrowserTestAgent();
        });

        var host = builder.Build();

        return host;
    }
}
