using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sending
{
    internal class HerculesRecordsSendingDaemon : IDisposable
    {
        private readonly object startLock = new object();

        private readonly ILog log;
        private readonly IHerculesRecordsSendingJob job;

        private readonly CancellationTokenSource daemonCancellation;
        private Task daemonTask;

        public HerculesRecordsSendingDaemon(ILog log, IHerculesRecordsSendingJob job)
        {
            this.log = log;
            this.job = job;

            daemonCancellation = new CancellationTokenSource();
        }

        public long SentRecordsCount => job.SentRecordsCount;

        public long LostRecordsCount => job.LostRecordsCount;

        public void Initialize()
        {
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

            job.RunAsync().SilentlyContinue().GetAwaiter().GetResult();
        }

        private async Task StartAsync()
        {
            try
            {
                while (!daemonCancellation.IsCancellationRequested)
                {
                    await job.WaitNextOccurrenceAsync().ConfigureAwait(false);
                    await job.RunAsync(daemonCancellation.Token).ConfigureAwait(false);
                }
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