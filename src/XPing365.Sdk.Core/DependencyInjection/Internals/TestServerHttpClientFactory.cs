using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.DependencyInjection.Internals;

internal class TestServerHttpClientFactory(HttpClient httpClient) : IHttpClientFactory, IDisposable
{
    private readonly HttpClient _httpClient = httpClient.RequireNotNull(nameof(httpClient));
    private bool _disposedValue;

    public HttpClient CreateClient(string name)
    {
        return _httpClient;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
