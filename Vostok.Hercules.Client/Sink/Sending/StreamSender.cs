using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Primitives;
using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.StreamState;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal class StreamSender : IStreamSender
    {
        private readonly IStreamState state;
        private readonly IBufferSnapshotBatcher batcher;
        private readonly IRequestContentFactory contentFactory;
        private readonly IRequestSender sender;
        private readonly ILog log;

        public StreamSender(
            IStreamState state,
            IBufferSnapshotBatcher batcher,
            IRequestContentFactory contentFactory,
            IRequestSender sender,
            ILog log)
        {
            this.state = state;
            this.batcher = batcher;
            this.contentFactory = contentFactory;
            this.sender = sender;
            this.log = log;
        }

        public async Task<StreamSendResult> SendAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var snapshots = state
                .BufferPool
                .Select(x => x.TryMakeSnapshot())
                .Where(x => x != null && x.State.RecordsCount > 0)
                .ToArray();

            if (snapshots.Length == 0)
                return StreamSendResult.NothingToSend;

            foreach (var snapshot in batcher.Batch(snapshots))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var apiKeyProvider = state.Settings.ApiKeyProvider;

                if (!await PushAsync(state.StreamName, snapshot, apiKeyProvider, timeout, cancellationToken).ConfigureAwait(false))
                    return StreamSendResult.Failure;
            }

            return StreamSendResult.Success;
        }

        private static void RequestGarbageCollection(ArraySegment<BufferSnapshot> snapshots)
        {
            foreach (var snapshot in snapshots)
                snapshot.Parent.RequestGarbageCollection(snapshot.State);
        }

        private async Task<bool> PushAsync(
            string stream,
            ArraySegment<BufferSnapshot> snapshots,
            Func<string> apiKeyProvider,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            var body = contentFactory.CreateContent(snapshots, out var recordsCount);

            var sendingResult = await sender.FireAndForgetAsync(stream, body, timeout, apiKeyProvider, cancellationToken)
                .ConfigureAwait(false);

            var recordsLength = body.Length - sizeof(int);

            LogSendingResult(sendingResult, recordsCount, recordsLength, stream, sw.Elapsed);

            if (sendingResult.IsSuccessful)
            {
                state.Statistics.ReportSuccessfulSending(recordsCount, recordsLength);
                RequestGarbageCollection(snapshots);
                return true;
            }

            if (sendingResult.IsDefinitiveFailure)
            {
                state.Statistics.ReportSendingFailure(recordsCount, recordsLength);
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
    }
}