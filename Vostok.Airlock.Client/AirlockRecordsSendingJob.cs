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
        private readonly IAirlockRecordsSendingJobScheduler scheduler;
        private readonly IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools;
        private readonly IBufferSliceFactory bufferSlicer;
        private readonly byte[] messageBuffer;
        private readonly IRequestSender requestSender;

        private readonly Dictionary<string, int> attempts;
        private readonly Dictionary<string, Task> delays;

        private volatile int sentRecordsCounter;

        public AirlockRecordsSendingJob(
            ILog log,
            IAirlockRecordsSendingJobScheduler scheduler,
            IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools,
            IBufferSliceFactory bufferSlicer,
            byte[] messageBuffer,
            IRequestSender requestSender)
        {
            this.log = log;
            this.scheduler = scheduler;
            this.bufferPools = bufferPools;
            this.bufferSlicer = bufferSlicer;
            this.messageBuffer = messageBuffer;
            this.requestSender = requestSender;

            attempts = new Dictionary<string, int>();
            delays = new Dictionary<string, Task>();
        }

        public int SentRecordsCount => sentRecordsCounter;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            foreach (var pair in bufferPools)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stream = pair.Key;
                var bufferPoolLazy = pair.Value;

                if (delays.TryGetValue(stream, out var delay) && !delay.IsCompleted)
                {
                    continue;
                }

                var sw = Stopwatch.StartNew();

                var isSuccess = await PushAsync(stream, bufferPoolLazy.Value, cancellationToken).ConfigureAwait(false);

                attempts[stream] = CalcalateAttempt(stream, isSuccess);

                var jobState = new AirlockRecordsSendingJobState
                {
                    IsSuccess = isSuccess,
                    Attempt = attempts[stream],
                    Elapsed = sw.Elapsed
                };

                var schedule = scheduler.GetDelayToNextOccurrence(jobState);

                delays[stream] = schedule.WaitNextOccurrenceAsync(cancellationToken);
            }
        }

        public Task WaitNextOccurrenceAsync()
        {
            return delays.Count != 0 ? Task.WhenAny(delays.Select(x => x.Value)) : Task.CompletedTask;
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

        private int CalcalateAttempt(string stream, bool result)
        {
            return result
                ? 0
                : attempts.TryGetValue(stream, out var attempt)
                    ? attempt + 1
                    : 1;
        }
    }
}