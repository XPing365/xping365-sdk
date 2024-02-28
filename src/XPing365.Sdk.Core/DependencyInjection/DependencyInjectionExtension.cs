using Microsoft.Extensions.DependencyInjection;
using Polly;
using XPing365.Sdk.Core.Configurations;
using XPing365.Sdk.Core.DependencyInjection.Internals;
using XPing365.Sdk.Core.HeadlessBrowser;
using XPing365.Sdk.Core.HeadlessBrowser.Internals;
using XPing365.Sdk.Core.Session;

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
    public static IServiceCollection AddHttpClients(
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

        return services;
    }

    public static IServiceCollection AddTestServerHttpClient(
        this IServiceCollection services, Func<HttpClient> httpClientBuilder)
    {
        services.AddTransient<IHttpClientFactory>(implementationFactory: 
            serviceProvider => new TestServerHttpClientFactory(httpClientBuilder()));

        return services;
    }

    /// <summary>
    /// This extension method adds the necessary factory service to create headless browser instance into your 
    /// application’s service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns><see cref="IServiceCollection"/> object.</returns>
    public static IServiceCollection AddBrowserClients(
        this IServiceCollection services)
    {
        services.AddTransient<IHeadlessBrowserFactory, DefaultHeadlessBrowserFactory>();

        return services;
    }

    public static IServiceCollection AddTestAgent(
        this IServiceCollection services,
        string name,
        Func<TestAgent, TestAgent> builder)
    {
        services.AddTransient<ITestSessionBuilder, TestSessionBuilder>();
        services.AddKeyedTransient(
            serviceKey: name,
            implementationFactory: (IServiceProvider provider, object? serviceKey) =>
            {
                var agent = new TestAgent(provider);
                return builder(agent);
            });

        return services;
    }

    public static IServiceCollection AddTestAgent(this IServiceCollection services)
    {
        services.AddTransient<ITestSessionBuilder, TestSessionBuilder>();
        services.AddTransient(implementationFactory: (IServiceProvider provider) => new TestAgent(provider));

        return services;
    }
}
