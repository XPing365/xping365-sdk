using System.Diagnostics;

namespace XPing365.Shared
{
    internal class InstrumentationLog : IDisposable
    {
        private readonly ThreadLocal<Stopwatch> stopper = new(() => new Stopwatch());
        private readonly Action<InstrumentationLog>? callback;

        public InstrumentationLog(Action<InstrumentationLog>? callback = null, bool startStopper = true)
        {
            this.callback = callback;

            if (startStopper)
            {
                this.stopper.Value!.Restart();
            }
        }

        public long ElapsedMilliseconds { get { return this.stopper.Value!.ElapsedMilliseconds; } }

        public long ElapsedTicks { get { return this.stopper.Value!.ElapsedTicks; } }

        public TimeSpan ElapsedTime { get { return this.stopper.Value!.Elapsed; } }

        public void Restart()
        {
            this.stopper.Value!.Restart();
        }

        public void Dispose()
        {
            this.stopper.Value!.Stop();
            this.callback?.Invoke(this);
        }
    }
}
