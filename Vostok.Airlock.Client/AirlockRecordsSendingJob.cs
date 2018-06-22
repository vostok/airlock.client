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

                attempts[stream] = CalculateAttempt(stream, isSuccess);

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
            var buffers = bufferPool.MakeSnapshot();
            var snapshots = buffers.Select(x => x.MakeSnapshot());

            var isSuccess = true;

            foreach (var context in BuildMessages(snapshots))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!await PushAsync(stream, context, cancellationToken).ConfigureAwait(false))
                {
                    isSuccess = false;
                }
            }

            return isSuccess;
        }

        private IEnumerable<RequestMessageBuildingContext> BuildMessages(IEnumerable<BufferSnapshot> snapshots)
        {
            RequestMessageBuildingContext context = null;

            foreach (var slice in snapshots.SelectMany(snapshot => bufferSlicer.Cut(snapshot)).OrderByDescending(x => x.Length))
            {
                if (context == null)
                {
                    context = new RequestMessageBuildingContext(messageBuffer);
                }

                if (context.Builder.TryAppend(slice))
                {
                    continue;
                }

                yield return context;

                context = null;
            }
        }

        private async Task<bool> PushAsync(string stream, RequestMessageBuildingContext context, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            if (!await requestSender.SendAsync(stream, context.Message, cancellationToken).ConfigureAwait(false))
            {
                log.Warn($"Sending to stream {stream} failed after {sw.Elapsed}");

                return false;
            }

            foreach (var slice in context.Slices)
            {
                slice.Parrent.RequestGarbageCollection(slice.Offset, slice.Length, slice.RecordsCount);
            }

            var recordsCount = context.Slices.Sum(x => x.RecordsCount);

            sentRecordsCounter += recordsCount;

            log.Info($"Successfully sent {recordsCount} records to stream {stream} in {sw.Elapsed}");

            return true;
        }

        private int CalculateAttempt(string stream, bool result)
        {
            return result
                ? 0
                : attempts.TryGetValue(stream, out var attempt)
                    ? attempt + 1
                    : 1;
        }
    }
}