using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Primitives;
using Vostok.Hercules.Client.Sink;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sending
{
    internal class HerculesRecordsSendingJob : IHerculesRecordsSendingJob
    {
        private readonly WeakReference<IReadOnlyDictionary<string, Lazy<IBufferPool>>> bufferPools;
        private readonly ILog log;
        private readonly IHerculesRecordsSendingJobScheduler scheduler;
        private readonly IBufferSnapshotBatcher batcher;
        private readonly IBodyFormatter formatter;
        private readonly IRequestSender requestSender;
        private readonly TimeSpan timeout;

        private readonly Dictionary<string, Task> delays;

        private long sentRecordsCounter;
        private long lostRecordsCounter;

        public HerculesRecordsSendingJob(
            IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools,
            IHerculesRecordsSendingJobScheduler scheduler,
            IBufferSnapshotBatcher batcher,
            IBodyFormatter formatter,
            IRequestSender requestSender,
            ILog log,
            TimeSpan timeout)
        {
            this.bufferPools = new WeakReference<IReadOnlyDictionary<string, Lazy<IBufferPool>>>(bufferPools);

            this.log = log;
            this.scheduler = scheduler;
            this.batcher = batcher;
            this.formatter = formatter;
            this.requestSender = requestSender;
            this.timeout = timeout;

            delays = new Dictionary<string, Task>();
        }

        public long SentRecordsCount => Interlocked.Read(ref sentRecordsCounter);

        public long LostRecordsCount => Interlocked.Read(ref lostRecordsCounter);

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            if (!bufferPools.TryGetTarget(out var pools))
                throw new OperationCanceledException();

            foreach (var pair in pools)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stream = pair.Key;
                var bufferPoolLazy = pair.Value;

                if (delays.TryGetValue(stream, out var delay) && !delay.IsCompleted)
                    continue;

                var sw = Stopwatch.StartNew();

                var bufferPool = bufferPoolLazy.Value;

                bufferPool.NeedToFlushEvent.Reset();

                var sendingResult = await PushAsync(stream, bufferPool, cancellationToken).ConfigureAwait(false);

                var delayTime = scheduler.GetDelayToNextOccurrence(stream, sendingResult, sw.Elapsed);

                delays[stream] = Task.WhenAny(bufferPool.NeedToFlushEvent, Task.Delay(delayTime, cancellationToken));
            }
        }

        public Task WaitNextOccurrenceAsync() =>
            delays.Count != 0 ? Task.WhenAny(delays.Select(x => x.Value)) : Task.CompletedTask;

        private async Task<bool> PushAsync(string stream, IBufferPool bufferPool, CancellationToken cancellationToken)
        {
            var buffers = bufferPool.MakeSnapshot();

            if (buffers == null)
                return false;

            var snapshots = buffers
                .Select(x => x.MakeSnapshot())
                .Where(x => x.State.RecordsCount > 0)
                .ToArray();

            var sendAny = false;

            foreach (var snapshot in batcher.Batch(snapshots))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var apiKeyProvider = bufferPool.Settings.ApiKeyProvider;

                if (!await PushAsync(stream, snapshot, apiKeyProvider, cancellationToken).ConfigureAwait(false))
                    return false;

                sendAny = true;
            }

            return sendAny;
        }

        private async Task<bool> PushAsync(
            string stream,
            ArraySegment<BufferSnapshot> snapshots,
            Func<string> apiKeyProvider,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            var body = formatter.GetContent(snapshots, out var recordsCount);

            var sendingResult = await requestSender.SendAsync(stream, body, timeout, apiKeyProvider, cancellationToken)
                .ConfigureAwait(false);

            LogSendingResult(sendingResult, recordsCount, body.Length, stream, sw.Elapsed);

            switch (sendingResult)
            {
                case RequestSendingResult.Success:
                    sentRecordsCounter += recordsCount;
                    break;
                case RequestSendingResult.DefinitiveFailure:
                    lostRecordsCounter += recordsCount;
                    break;
                case RequestSendingResult.IntermittentFailure:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sendingResult));
            }

            foreach (var snapshot in snapshots)
                snapshot.Parent.RequestGarbageCollection(snapshot.State);

            return true;
        }

        private void LogSendingResult(RequestSendingResult result, int recordsCount, long bytesCount, string stream, TimeSpan elapsed)
        {
            if (result == RequestSendingResult.Success)
            {
                log.Info($"Sending {recordsCount.ToString()} records of size {DataSize.FromBytes(bytesCount).ToString()} to stream {stream} succeeded in {elapsed.ToString()}");
            }
            else
            {
                log.Warn($"Sending {recordsCount.ToString()} records of size {DataSize.FromBytes(bytesCount).ToString()} to stream {stream} failed after {elapsed.ToString()}");
            }
        }
    }
}