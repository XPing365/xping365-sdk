namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

public abstract class HeadlessBrowserClient(BrowserContext context) : IDisposable
{
    private bool disposedValue;

    public BrowserContext Context { get; init; } = context;

    public abstract Task<WebPage> GetAsync(
        Uri url,
        CancellationToken cancellationToken = default);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
