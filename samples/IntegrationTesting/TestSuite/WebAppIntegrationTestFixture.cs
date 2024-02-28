using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using XPing365.Sdk.Core.DependencyInjection;
using XPing365.Sdk.Core.Session;

namespace NUnitTestProject.TestSuite;

public abstract class WebAppIntegrationTestFixture : IDisposable
{
    private readonly IHost _hostInstance;
    private readonly WebAppFactory _factory = null!;
    private bool _disposedValue;

    protected IServiceProvider ServiceProvider => _hostInstance.Services;
    protected Uri TestServer { get; } = new Uri("http://localhost/");

    protected WebAppIntegrationTestFixture()
    {
        _factory = new WebAppFactory();
        _hostInstance = CreateHost(_factory);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _factory.Dispose();
                _hostInstance.Dispose();
            }

            _disposedValue = true;
        }
    }

    private static IHost CreateHost(WebAppFactory factory)
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(implementationInstance: Mock.Of<IProgress<TestStep>>());
            services.AddTestServerHttpClient(() => factory.CreateDefaultClient());
            services.AddTestAgent();
        });

        var host = builder.Build();

        return host;
    }
}
