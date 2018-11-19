using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordsSendingDaemon : IDisposable
    {
        private readonly ILog log;
        private readonly IHerculesRecordsSendingJob job;

        private readonly Task daemonTask;
        private readonly CancellationTokenSource daemonCancellation;

        public HerculesRecordsSendingDaemon(ILog log, IHerculesRecordsSendingJob job)
        {
            this.log = log;
            this.job = job;

            daemonCancellation = new CancellationTokenSource();
            daemonTask = Task.Run(StartAsync, daemonCancellation.Token);
        }

        public long SentRecordsCount => job.SentRecordsCount;
        
        public long LostRecordsCount => job.LostRecordsCount;

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