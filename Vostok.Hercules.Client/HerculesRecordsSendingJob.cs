using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Binary;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordsSendingJob : IHerculesRecordsSendingJob
    {
        private readonly ILog log;
        private readonly IHerculesRecordsSendingJobScheduler scheduler;
        private readonly IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools;
        private readonly IRequestSender requestSender;
        private readonly TimeSpan timeout;

        private readonly Dictionary<string, Task> delays;

        private long sentRecordsCounter;
        private long lostRecordsCounter;

        public HerculesRecordsSendingJob(
            ILog log,
            IHerculesRecordsSendingJobScheduler scheduler,
            IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools,
            IRequestSender requestSender,
            TimeSpan timeout)
        {
            this.log = log;
            this.scheduler = scheduler;
            this.bufferPools = bufferPools;
            this.requestSender = requestSender;
            this.timeout = timeout;

            delays = new Dictionary<string, Task>();
        }

        public long SentRecordsCount => Interlocked.Read(ref sentRecordsCounter);

        public long LostRecordsCount => Interlocked.Read(ref lostRecordsCounter);

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            foreach (var pair in bufferPools)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stream = pair.Key;
                var bufferPoolLazy = pair.Value;

                if (delays.TryGetValue(stream, out var delay) && !delay.IsCompleted)
                    continue;

                var sw = Stopwatch.StartNew();

                var sendingResult = await PushAsync(stream, bufferPoolLazy.Value, cancellationToken).ConfigureAwait(false);

                var schedule = scheduler.GetDelayToNextOccurrence(stream, sendingResult, sw.Elapsed);

                delays[stream] = schedule.WaitNextOccurrenceAsync(cancellationToken);
            }
        }

        public Task WaitNextOccurrenceAsync() =>
            delays.Count != 0 ? Task.WhenAny(delays.Select(x => x.Value)) : Task.CompletedTask;

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

        private static unsafe void SetRecordsCount(byte[] buffer, int recordsCount)
        {
            fixed (byte* b = buffer)
                *(int*) b = EndiannessConverter.Convert(recordsCount, Endianness.Big);
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