using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Extensions;

namespace Vostok.Airlock.Client
{
    internal class AirlockRecordsSendingDaemon : IDisposable
    {
        private readonly ILog log;
        private readonly IAirlockRecordsSendingJob job;

        private readonly Task daemonTask;
        private readonly CancellationTokenSource daemonCancellation;

        public AirlockRecordsSendingDaemon(ILog log, IAirlockRecordsSendingJob job)
        {
            this.log = log;
            this.job = job;

            daemonCancellation = new CancellationTokenSource();
            daemonTask = Task.Run(StartAsync, daemonCancellation.Token);
        }

        public int SentRecordsCount => job.SentRecordsCount;

        public void Dispose()
        {
            daemonCancellation.Cancel();
            daemonTask.SilentlyContinue().GetAwaiter().GetResult();
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