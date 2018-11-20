using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordsSendingJob : IHerculesRecordsSendingJob
    {
        private readonly ILog log;
        private readonly IHerculesRecordsSendingJobScheduler scheduler;
        private readonly IReadOnlyDictionary<string, Lazy<IBufferPool>> bufferPools;
        private readonly IBufferSliceFactory bufferSlicer;
        private readonly byte[] messageBuffer;
        private readonly IRequestSender requestSender;

        private readonly Dictionary<string, Task> delays;

        private long sentRecordsCounter;
        private long lostRecordsCounter;

        public HerculesRecordsSendingJob(
            ILog log,
            IHerculesRecordsSendingJobScheduler scheduler,
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
            
            foreach (var context in BuildMessages(snapshots))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (context.Slices.Count == 0)
                    continue;

                if (!await PushAsync(stream, context, cancellationToken).ConfigureAwait(false))
                    return false;
                
                sendAny = true;
            }

            return sendAny;

        }

        private IEnumerable<RequestMessageBuildingContext> BuildMessages(IEnumerable<BufferSnapshot> snapshots)
        {
            RequestMessageBuildingContext context = null;

            var slices = snapshots
                .SelectMany(snapshot => bufferSlicer.Cut(snapshot))
                .OrderByDescending(x => x.Length);

            foreach (var slice in slices)
            {
                if (context == null)
                    context = new RequestMessageBuildingContext(messageBuffer);

                if (context.Builder.IsFull)
                    break;
                
                if (context.Builder.TryAppend(slice))
                    continue;

                yield return context;

                context = null;
            }

            if (context != null)
                yield return context;
        }

        private async Task<bool> PushAsync(string stream, RequestMessageBuildingContext context, CancellationToken cancellationToken)
        {   
            var sw = Stopwatch.StartNew();

            var sendingResult = await requestSender.SendAsync(stream, context.Message, cancellationToken).ConfigureAwait(false);

            var recordsCount = context.Slices.Sum(x => x.RecordsCount);

            LogSendingResult(sendingResult, recordsCount, context.Message.Count, stream, sw.Elapsed);

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

            foreach (var slice in context.Slices)
                slice.Parent.RequestGarbageCollection(slice.Offset, slice.Length, slice.RecordsCount);

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