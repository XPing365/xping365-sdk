using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XPing365.Sdk.Core.DependencyInjection;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Availability.TestActions;
using Microsoft.Extensions.Logging;

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
            services.AddHttpClients();
            services.AddBrowserClients();
            services.AddTestAgent(
                name: "HttpClient", builder: (TestAgent agent) =>
                {
                    agent.Container = new Pipeline(
                        name: "Availability pipeline (HttpClient)",
                        components: [
                            new DnsLookup(),
                            new IPAddressAccessibilityCheck(),
                            new HttpRequestSender(Client.HttpClient)
                        ]);
                    return agent;
                });
            services.AddTestAgent(
                name: "BrowserClient", builder: (TestAgent agent) =>
                {
                    agent.Container = new Pipeline(
                        name: "Availability pipeline (BrowserClient)",
                        components: [
                            new DnsLookup(),
                            new IPAddressAccessibilityCheck(),
                            new HttpRequestSender(Client.HeadlessBrowser)
                        ]);
                    return agent;
                });
        });
        builder.ConfigureLogging(logging =>
        {
            logging.AddFilter(typeof(HttpClient).FullName, LogLevel.Warning);
        });

        var host = builder.Build();

        return host;
    }
}
