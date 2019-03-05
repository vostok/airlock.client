using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Primitives;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Gateway;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Worker
{
    internal class HerculesRecordsSendingJob : IHerculesRecordsSendingJob
    {
        private readonly IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools;
        private readonly WeakReference<IHerculesSink> sinkWeakReference;
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
            IHerculesSink sink,
            IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools,
            IHerculesRecordsSendingJobScheduler scheduler,
            IBufferSnapshotBatcher batcher,
            IBodyFormatter formatter,
            IRequestSender requestSender,
            ILog log,
            TimeSpan timeout)
        {
            sinkWeakReference = new WeakReference<IHerculesSink>(sink);

            this.bufferPools = bufferPools;
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
            var sendAny = false;

            foreach (var pair in bufferPools)
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

                if (sendingResult)
                    sendAny = true;

                var delayTime = scheduler.GetDelayToNextOccurrence(stream, sendingResult, sw.Elapsed);

                delays[stream] = Task.WhenAny(bufferPool.NeedToFlushEvent, Task.Delay(delayTime, cancellationToken));
            }

            if (!sendAny && SinkIsCollectedByGC())
                throw new OperationCanceledException();
        }

        public Task WaitNextOccurrenceAsync() =>
            delays.Count != 0 ? Task.WhenAny(delays.Select(x => x.Value)) : Task.CompletedTask;

        private static void RequestGarbageCollection(ArraySegment<BufferSnapshot> snapshots)
        {
            foreach (var snapshot in snapshots)
                snapshot.Parent.RequestGarbageCollection(snapshot.State);
        }

        private async Task<bool> PushAsync(string stream, IBufferPool bufferPool, CancellationToken cancellationToken)
        {
            var buffers = bufferPool.MakeSnapshot();

            if (buffers == null)
                return false;

            var snapshots = buffers
                .Select(x => x.MakeSnapshot())
                .Where(x => x.State.RecordsCount > 0)
                .ToArray();

            if (snapshots.Length == 0)
                return false;

            foreach (var snapshot in batcher.Batch(snapshots))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var apiKeyProvider = bufferPool.Settings.ApiKeyProvider;

                if (!await PushAsync(stream, snapshot, apiKeyProvider, cancellationToken).ConfigureAwait(false))
                    return false;
            }

            return true;
        }

        private async Task<bool> PushAsync(
            string stream,
            ArraySegment<BufferSnapshot> snapshots,
            Func<string> apiKeyProvider,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            var body = formatter.CreateContent(snapshots, out var recordsCount);

            var sendingResult = await requestSender.FireAndForgetAsync(stream, body, timeout, apiKeyProvider, cancellationToken)
                .ConfigureAwait(false);

            LogSendingResult(sendingResult, recordsCount, body.Length, stream, sw.Elapsed);

            if (sendingResult.IsSuccessful)
            {
                sentRecordsCounter += recordsCount;
                RequestGarbageCollection(snapshots);
                return true;
            }

            if (sendingResult.IsDefinitiveFailure)
            {
                lostRecordsCounter += recordsCount;
                RequestGarbageCollection(snapshots);
            }

            return false;
        }

        private void LogSendingResult(RequestSendingResult result, int recordsCount, long bytesCount, string stream, TimeSpan elapsed)
        {
            if (result.IsSuccessful)
            {
                log.Info(
                    "Sending {RecordsCount} records of size {RecordsSize} to stream {StreamName} succeeded in {ElapsedTime}",
                    recordsCount,
                    DataSize.FromBytes(bytesCount).ToString(),
                    stream,
                    elapsed);
            }
            else
            {
                log.Warn(
                    "Sending {RecordsCount} records of size {RecordsSize} to stream {StreamName} failed after {ElapsedTime} with status {Status} and code {Code}",
                    recordsCount,
                    DataSize.FromBytes(bytesCount).ToString(),
                    stream,
                    elapsed,
                    result.Status,
                    result.Code);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SinkIsCollectedByGC()
        {
            return !sinkWeakReference.TryGetTarget(out _);
        }
    }
}