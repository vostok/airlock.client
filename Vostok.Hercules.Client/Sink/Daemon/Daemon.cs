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
        private readonly CancellationTokenSource schedulerCancellation;

        private volatile IScheduler scheduler;
        private volatile Task schedulerTask;

        public Daemon(IScheduler scheduler)
        {
            this.scheduler = scheduler;

            schedulerCancellation = new CancellationTokenSource();
            state = new AtomicInt(NotInitialized);
        }

        public void Initialize()
        {
            if (state == NotInitialized && state.TryIncreaseTo(Initialized))
            {
                Interlocked.Exchange(ref schedulerTask, Task.Run(() => scheduler?.RunAsync(schedulerCancellation.Token)));
            }
        }

        public void Dispose()
        {
            if (state.TryIncreaseTo(Disposed))
            {
                schedulerCancellation.Cancel();
                schedulerTask?.SilentlyContinue()?.GetAwaiter().GetResult();
                schedulerCancellation.Dispose();
                scheduler = null;
            }
        }
    }
}