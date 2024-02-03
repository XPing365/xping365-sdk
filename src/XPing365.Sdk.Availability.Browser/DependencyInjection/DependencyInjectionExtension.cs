using Microsoft.Extensions.DependencyInjection;
using XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;
using XPing365.Sdk.Core.Components.Session;

namespace XPing365.Sdk.Availability.Browser.DependencyInjection;

public static class DependencyInjectionExtension
{
    /// <summary>
    /// This extension method adds the BrowserTestAgent service and related services to your application’s service 
    /// collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">The <see cref="HttpClientConfiguration"/>.</param>
    /// <returns><see cref="IServiceCollection"/> object.</returns>
    public static IServiceCollection AddBrowserTestAgent(
        this IServiceCollection services)
    {
        services.AddTransient<ITestSessionBuilder, TestSessionBuilder>();
        services.AddTransient<IHeadlessBrowserFactory, HeadlessBrowserFactory>();
        services.AddTransient<BrowserTestAgent>();

        return services;
    }
}
