using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Extensions;

namespace Vostok.Airlock.Client
{
    internal class AirlockRecordsSendingJob : IAirlockRecordsSendingJob
    {
        private readonly ILog log;
        private readonly AirlockRecordsSendingJobSchedule schedule;
        private readonly IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools;
        private readonly IBufferSliceFactory bufferSlicer;
        private readonly byte[] messageBuffer;
        private readonly IRequestSender requestSender;

        private volatile int attemptsCounter;
        private volatile int sentRecordsCounter;

        public AirlockRecordsSendingJob(
            ILog log,
            AirlockRecordsSendingJobSchedule schedule,
            IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools,
            IBufferSliceFactory bufferSlicer,
            byte[] messageBuffer,
            IRequestSender requestSender)
        {
            this.log = log;
            this.schedule = schedule;
            this.bufferPools = bufferPools;
            this.bufferSlicer = bufferSlicer;
            this.messageBuffer = messageBuffer;
            this.requestSender = requestSender;
        }

        public IAirlockRecordsSendingJobSchedule Schedule => schedule;

        public int SentRecordsCount => sentRecordsCounter;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();
            
            var result = true;

            foreach (var pair in bufferPools)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stream = pair.Key;
                var bufferPoolLazy = pair.Value;

                if (!await PushAsync(stream, bufferPoolLazy.Value, cancellationToken).ConfigureAwait(false))
                {
                    result = false;
                }
            }

            attemptsCounter = result ? 0 : attemptsCounter + 1;

            var jobState = new AirlockRecordsSendingJobState
            {
                Result = result,
                Attempt = attemptsCounter,
                Elapsed = sw.Elapsed
            };

            schedule.SetLastJobRunningState(jobState);
        }

        private async Task<bool> PushAsync(string stream, IBufferPool bufferPool, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            var buffers = bufferPool.MakeSnapshot();
            var snapshots = buffers.Select(x => x.MakeSnapshot());

            var messages = BuildMessages(snapshots);

            var (result, recordsCount) = await PushAsync(stream, messages, cancellationToken).ConfigureAwait(false);

            if (!result)
            {
                log.Warn($"Sending to stream {stream} failed after {sw.Elapsed}.");

                return false;
            }

            log.Info($"Successfully sent {recordsCount} records to stream {stream} in {sw.Elapsed}.");

            sentRecordsCounter += recordsCount;

            foreach (var buffer in buffers)
            {
                buffer.RequestGarbageCollection();
            }

            return true;
        }

        private IEnumerable<RequestMessage> BuildMessages(IEnumerable<BufferSnapshot> snapshots)
        {
            var context = new RequestMessageBuildingContext(messageBuffer);

            foreach (var slice in snapshots.SelectMany(snapshot => bufferSlicer.Cut(snapshot)))
            {
                if (context.Appender.TryAppend(slice))
                {
                    continue;
                }

                yield return context.Build();

                context.Reset();
            }
        }

        private async Task<(bool result, int recordsCount)> PushAsync(string stream, IEnumerable<RequestMessage> messages, CancellationToken cancellationToken)
        {
            var recordsCount = 0;

            foreach (var message in messages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (await requestSender.SendAsync(stream, message.Message, cancellationToken).ConfigureAwait(false))
                {
                    recordsCount += message.RecordsCount;
                }
                else
                {
                    return (false, 0);
                }
            }

            return (true, recordsCount);
        }
    }
}