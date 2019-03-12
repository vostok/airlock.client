using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Daemon
{
    internal class RecordsSendingDaemon : IRecordsSendingDaemon
    {
        private readonly object startLock = new object();

        private readonly ILog log;
        private readonly IScheduler scheduler;

        private readonly CancellationTokenSource daemonCancellation;
        private Task daemonTask;

        public RecordsSendingDaemon(ILog log, IScheduler scheduler)
        {
            this.log = log;
            this.scheduler = scheduler;

            daemonCancellation = new CancellationTokenSource();
        }

        public void Initialize()
        {
            // ReSharper disable once InvertIf
            if (daemonTask == null)
            {
                lock (startLock)
                {
                    if (daemonTask != null)
                        return;

                    daemonTask = Task.Run(StartAsync, daemonCancellation.Token);
                }
            }
        }

        public void Dispose()
        {
            daemonCancellation.Cancel();
            daemonTask?.SilentlyContinue()?.GetAwaiter().GetResult();
            daemonCancellation.Dispose();

            scheduler.Dispose();
        }

        private async Task StartAsync()
        {
            try
            {
                await scheduler.RunAsync(daemonCancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                log.Fatal(exception);
            }
        }
    }
}