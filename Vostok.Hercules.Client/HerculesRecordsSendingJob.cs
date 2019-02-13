using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Binary;
using Vostok.Commons.Primitives;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordsSendingJob : IHerculesRecordsSendingJob
    {
        private readonly WeakReference<IReadOnlyDictionary<string, Lazy<IBufferPool>>> bufferPools;
        private readonly ILog log;
        private readonly IHerculesRecordsSendingJobScheduler scheduler;
        private readonly IRequestSender requestSender;
        private readonly TimeSpan timeout;

        private readonly Dictionary<string, Task> delays;

        private long sentRecordsCounter;
        private long lostRecordsCounter;

        public HerculesRecordsSendingJob(
            IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools,
            IHerculesRecordsSendingJobScheduler scheduler,
            IRequestSender requestSender,
            ILog log,
            TimeSpan timeout)
        {
            this.bufferPools = new WeakReference<IReadOnlyDictionary<string, Lazy<IBufferPool>>>(bufferPools);

            this.log = log;
            this.scheduler = scheduler;
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

        private static unsafe void SetRecordsCount(byte[] buffer, int recordsCount)
        {
            if (buffer.Length < sizeof(int))
                throw new ArgumentException($"Buffer length {buffer.Length} is less than {sizeof(int)}.");

            fixed (byte* b = buffer)
                *(int*)b = EndiannessConverter.Convert(recordsCount, Endianness.Big);
        }

        private async Task<bool> PushAsync(string stream, IBufferPool bufferPool, CancellationToken cancellationToken)
        {
            var buffers = bufferPool.MakeSnapshot();

            if (buffers == null)
                return false;

            var snapshots = buffers.Select(x => x.MakeSnapshot());

            var sendAny = false;

            foreach (var snapshot in snapshots)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (snapshot.State.RecordsCount == 0)
                    continue;

                if (!await PushAsync(stream, snapshot, cancellationToken).ConfigureAwait(false))
                    return false;

                sendAny = true;
            }

            return sendAny;
        }

        private async Task<bool> PushAsync(string stream, BufferSnapshot snapshot, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            var recordsCount = snapshot.State.RecordsCount;

            SetRecordsCount(snapshot.Buffer, recordsCount);

            var sendingResult = await requestSender.SendAsync(stream, snapshot.Data, timeout, cancellationToken).ConfigureAwait(false);

            LogSendingResult(sendingResult, recordsCount, snapshot.State.Length, stream, sw.Elapsed);

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

            snapshot.Parent.RequestGarbageCollection(snapshot.State);

            return true;
        }

        private void LogSendingResult(RequestSendingResult result, int recordsCount, int bytesCount, string stream, TimeSpan elapsed)
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