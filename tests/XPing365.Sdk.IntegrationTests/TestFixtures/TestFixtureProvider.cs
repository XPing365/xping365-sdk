using Microsoft.Extensions.Hosting;
using XPing365.Sdk.Availability.Extensions;

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
            services.AddAvailabilityTestAgent();
        });

        var host = builder.Build();

        return host;
    }
}
