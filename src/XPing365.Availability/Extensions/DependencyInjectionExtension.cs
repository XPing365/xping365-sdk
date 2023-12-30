using Microsoft.Extensions.DependencyInjection;
using Polly;
using XPing365.Availability.Configurations;
using XPing365.Availability.TestSteps;

namespace XPing365.Availability.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddAvailabilityTestAgent(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClientConfiguration>? configuration = null)
    {
        HttpClientConfiguration httpClientConfiguration = new();
        configuration?.Invoke(services.BuildServiceProvider(), httpClientConfiguration);

        services.AddHttpClient(SendHttpRequest.HttpClientWithNoRetryAndNoFollowRedirect)
                .ConfigurePrimaryHttpMessageHandler(configureHandler =>
                {
                    return new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = httpClientConfiguration.PooledConnectionLifetime,
                        AllowAutoRedirect = false,
                        UseCookies = false, // Set the cookie manually instead from the CookieContainer
                    };
                });

        services.AddHttpClient(SendHttpRequest.HttpClientWithNoRetryAndFollowRedirect)
                .ConfigurePrimaryHttpMessageHandler(configureHandler =>
                {
                    return new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = httpClientConfiguration.PooledConnectionLifetime,
                        AllowAutoRedirect = true,
                        UseCookies = false, // Set the cookie manually instead from the CookieContainer
                    };
                });

        services.AddHttpClient(SendHttpRequest.HttpClientWithRetryAndNoFollowRedirect)
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

        services.AddHttpClient(SendHttpRequest.HttpClientWithRetryAndFollowRedirect)
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

        services.AddTransient<AvailabilityTestAgent>();
        
        return services;
    }
}
