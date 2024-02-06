﻿using Microsoft.Extensions.DependencyInjection;
using Polly;
using XPing365.Sdk.Availability.Configurations;
using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Core.Components.Session;

namespace XPing365.Sdk.Availability.DependencyInjection;

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

        services.AddTransient<ITestSessionBuilder, TestSessionBuilder>();
        services.AddTransient<HttpClientTestAgent>();

        return services;
    }
}
