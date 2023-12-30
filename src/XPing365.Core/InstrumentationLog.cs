using System.Diagnostics;

namespace XPing365.Core;

public class InstrumentationLog : IDisposable
{
    private bool _isDisposed;
    private readonly Stopwatch _stopper = new();
    private readonly Action<InstrumentationLog>? _callback;
    private DateTime _startTime;

    public InstrumentationLog(Action<InstrumentationLog>? callback = null, bool startStopper = true)
    {
        _callback = callback;

        if (startStopper)
        {
            _startTime = DateTime.UtcNow;
            _stopper.Restart();
        }
    }

    public bool IsRunning => _stopper.IsRunning;
    public long ElapsedMilliseconds => _stopper.ElapsedMilliseconds;
    public long ElapsedTicks => _stopper.ElapsedTicks;
    public TimeSpan ElapsedTime => _stopper.Elapsed;
    public DateTime StartTime => _startTime;

    public void Restart()
    {
        _startTime = DateTime.UtcNow;
        _stopper.Restart();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            _stopper.Stop();
            _callback?.Invoke(this);
        }

        _isDisposed = true;
    }
}
