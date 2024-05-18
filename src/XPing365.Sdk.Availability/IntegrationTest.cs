using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.DependencyInjection;

namespace XPing365.Sdk.Availability;

/// <summary>
/// Provides a base class for integration testing within the XPing365 SDK environment.
/// </summary>
/// <typeparam name="TEntryPoint">
/// The entry point class of the application under test, typically the Startup class.
/// </typeparam>
/// <remarks>
/// This abstract class is designed to streamline the setup and configuration of the XPing365 SDK for integration 
/// testing purposes. It leverages lazy initialization to ensure that resources are created efficiently and only when 
/// needed. The class implements IDisposable to allow for proper cleanup of resources when testing is complete.
/// </remarks>
public abstract class IntegrationTest<TEntryPoint> : IDisposable where TEntryPoint : class
{
    private readonly Lazy<IHost> _lazyHost;
    private readonly Lazy<WebApplicationFactory<TEntryPoint>> _lazyFactory;
    private readonly Lazy<TestAgent> _lazyTestAgent;

    private bool _disposedValue;

    /// <summary>
    /// Gets the service provider from the lazily initialized host.
    /// </summary>
    protected IServiceProvider ServiceProvider => _lazyHost.Value.Services;

    /// <summary>
    /// Gets an instance of the WebApplicationFactory using lazy initialization.
    /// </summary>
    /// <value>
    /// The initialized instance of WebApplicationFactory.
    /// </value>
    protected WebApplicationFactory<TEntryPoint> WebApplicationFactory => _lazyFactory.Value;

    /// <summary>
    /// Gets the TestAgent from the lazily initialized value.
    /// </summary>
    protected TestAgent TestAgent => _lazyTestAgent.Value;

    /// <summary>
    /// Initializes a new instance of the IntegrationTest class.
    /// </summary>
    protected IntegrationTest()
    {
        _lazyHost = new(valueFactory: CreateHost);
        _lazyFactory = new(valueFactory: CreateFactory);
        _lazyTestAgent = new(valueFactory: ServiceProvider.GetRequiredService<TestAgent>);
    }

    /// <summary>
    /// Releases the resources used by the IntegrationTest.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Instantiates a new WebApplicationFactory of type TEntryPoint.
    /// </summary>
    /// <remarks>
    /// This method creates a new instance of the WebApplicationFactory class, which provides a test server for the 
    /// application. TEntryPoint is the entry point of the application, typically the Startup class, which configures 
    /// services. This method can be overridden in derived class to customize the instantiation of the 
    /// WebApplicationFactory.
    /// </remarks>
    /// <returns>
    /// A new instance of WebApplicationFactory configured for integration testing.
    /// </returns>
    protected virtual WebApplicationFactory<TEntryPoint> CreateFactory()
    {
        return new WebApplicationFactory<TEntryPoint>();
    }

    /// <summary>
    /// Configures the services collection for this test fixture class.
    /// </summary>
    /// <remarks>
    /// This method sets up the necessary services for the XPing 365 SDK to execute tests. It includes an HTTP client 
    /// factory for the test server and the addition of a test agent.
    /// </remarks>
    /// <param name="services">
    /// The IServiceCollection that holds the service registrations for running the tests.
    /// </param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddTestServerHttpClientFactory(WebApplicationFactory.CreateClient);
        services.AddTestAgent();
    }

    /// <summary>
    /// Releases the resources used by the IntegrationTest.
    /// </summary>
    /// <param name="disposing">A flag indicating whether to release the managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_lazyTestAgent.IsValueCreated)
                {
                    _lazyTestAgent.Value.Dispose();
                }

                if (_lazyFactory.IsValueCreated)
                {
                    _lazyFactory.Value.Dispose();
                }

                if (_lazyHost.IsValueCreated)
                {
                    _lazyHost.Value.Dispose();
                }
            }

            _disposedValue = true;
        }
    }

    private IHost CreateHost()
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(ConfigureServices);
        var host = builder.Build();

        return host;
    }
}
