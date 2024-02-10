using Microsoft.Extensions.DependencyInjection;
using Polly;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Configurations;
using XPing365.Sdk.Core.HeadlessBrowser;
using XPing365.Sdk.Core.HeadlessBrowser.Internals;

namespace XPing365.Sdk.Core.DependencyInjection;

public static class DependencyInjectionExtension
{
    /// <summary>
    /// This extension method adds the HttpClientTestAgent service and related services to your application’s service 
    /// collection and configures a named <see cref="HttpClient"/> clients.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">The <see cref="HttpClientConfiguration"/>.</param>
    /// <returns><see cref="IServiceCollection"/> object.</returns>
    public static IServiceCollection AddHttpClientTestAgent(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClientConfiguration>? configuration = null)
    {
        HttpClientConfiguration httpClientConfiguration = new();
        configuration?.Invoke(services.BuildServiceProvider(), httpClientConfiguration);

        services.AddHttpClient(HttpClientConfiguration.HttpClientWithNoRetryAndNoFollowRedirect)
                .ConfigurePrimaryHttpMessageHandler(configureHandler =>
                {
                    return new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = httpClientConfiguration.PooledConnectionLifetime,
                        AllowAutoRedirect = false,
                        UseCookies = false, // Set the cookie manually instead from the CookieContainer
                    };
                });

        services.AddHttpClient(HttpClientConfiguration.HttpClientWithNoRetryAndFollowRedirect)
                .ConfigurePrimaryHttpMessageHandler(configureHandler =>
                {
                    return new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = httpClientConfiguration.PooledConnectionLifetime,
                        AllowAutoRedirect = true,
                        UseCookies = false, // Set the cookie manually instead from the CookieContainer
                    };
                });

        services.AddHttpClient(HttpClientConfiguration.HttpClientWithRetryAndNoFollowRedirect)
                .ConfigurePrimaryHttpMessageHandler(configureHandler =>
                {
                    return new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = httpClientConfiguration.PooledConnectionLifetime,
                        AllowAutoRedirect = false,
                        UseCookies = false, // Set the cookie manually instead from the CookieContainer
                    };
                })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(
                    sleepDurations: httpClientConfiguration.SleepDurations))
                .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: httpClientConfiguration.HandledEventsAllowedBeforeBreaking,
                    durationOfBreak: httpClientConfiguration.DurationOfBreak));

        services.AddHttpClient(HttpClientConfiguration.HttpClientWithRetryAndFollowRedirect)
                .ConfigurePrimaryHttpMessageHandler(configureHandler =>
                {
                    return new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = httpClientConfiguration.PooledConnectionLifetime,
                        AllowAutoRedirect = true,
                        UseCookies = false, // Set the cookie manually instead from the CookieContainer
                    };
                })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(
                    sleepDurations: httpClientConfiguration.SleepDurations))
                .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: httpClientConfiguration.HandledEventsAllowedBeforeBreaking,
                    durationOfBreak: httpClientConfiguration.DurationOfBreak));

        services.AddTransient<ITestSessionBuilder, TestSessionBuilder>();
        services.AddTransient<TestAgent>();

        return services;
    }

    /// <summary>
    /// This extension method adds the BrowserTestAgent service and related services to your application’s service 
    /// collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns><see cref="IServiceCollection"/> object.</returns>
    public static IServiceCollection AddBrowserTestAgent(
        this IServiceCollection services)
    {
        services.AddTransient<ITestSessionBuilder, TestSessionBuilder>();
        services.AddTransient<IHeadlessBrowserFactory, DefaultHeadlessBrowserFactory>();
        services.AddTransient<TestAgent>();

        return services;
    }
}
