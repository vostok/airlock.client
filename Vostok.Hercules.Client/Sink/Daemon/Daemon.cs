using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Sink.Scheduler;

namespace Vostok.Hercules.Client.Sink.Daemon
{
    internal class Daemon : IDaemon
    {
        private const int NotInitialized = 0;
        private const int Initialized = 1;
        private const int Disposed = 2;

        private readonly AtomicInt state;
        private readonly AsyncManualResetEvent schedulerTaskBarrier;
        private readonly CancellationTokenSource schedulerCancellation;

        private volatile IScheduler scheduler;
        private volatile Task schedulerTask;

        public Daemon(IScheduler scheduler)
        {
            this.scheduler = scheduler;

            schedulerCancellation = new CancellationTokenSource();
            schedulerTaskBarrier = new AsyncManualResetEvent(false);
            state = new AtomicInt(NotInitialized);
        }

        public void Initialize()
        {
            if (state == NotInitialized && state.TryIncreaseTo(Initialized))
            {
                using (ExecutionContext.SuppressFlow())
                {
                    Interlocked.Exchange(ref schedulerTask, Task.Run(() => scheduler?.RunAsync(schedulerCancellation.Token)));
                }

                schedulerTaskBarrier.Set();
            }
        }

        public void Dispose()
        {
            var stateBefore = state.Exchange(Disposed);
            if (stateBefore == Disposed)
                return;

            // (iloktionov): Ensure that we don't miss scheduler task due to a race with Initialize().
            if (stateBefore == Initialized)
                schedulerTaskBarrier.GetAwaiter().GetResult();

            schedulerCancellation.Cancel();
            schedulerTask?.SilentlyContinue()?.GetAwaiter().GetResult();
            schedulerTask = null;
            scheduler = null;
            schedulerCancellation.Dispose();
        }
    }
}