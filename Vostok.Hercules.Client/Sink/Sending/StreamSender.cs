using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.StreamState;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal class StreamSender : IStreamSender
    {
        private readonly Func<string> apiKeyProvider;
        private readonly IStreamState state;
        private readonly IBufferSnapshotBatcher batcher;
        private readonly IRequestContentFactory contentFactory;
        private readonly IGateRequestSender sender;
        private readonly IGateResponseClassifier classifier;
        private readonly ILog log;

        public StreamSender(
            Func<string> apiKeyProvider,
            IStreamState state,
            IBufferSnapshotBatcher batcher,
            IRequestContentFactory contentFactory,
            IGateRequestSender sender,
            IGateResponseClassifier classifier,
            ILog log)
        {
            this.apiKeyProvider = apiKeyProvider;
            this.state = state;
            this.batcher = batcher;
            this.contentFactory = contentFactory;
            this.sender = sender;
            this.classifier = classifier;
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

                if (!await PushAsync(state.Name, snapshot, timeout, cancellationToken).ConfigureAwait(false))
                    return StreamSendResult.Failure;
            }

            return StreamSendResult.Success;
        }

        private static void RequestGarbageCollection(ArraySegment<BufferSnapshot> snapshots)
        {
            foreach (var snapshot in snapshots)
                snapshot.Source.ReportGarbage(snapshot.State);
        }

        private async Task<bool> PushAsync(
            string stream,
            ArraySegment<BufferSnapshot> snapshots,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            var apiKey = state?.Settings?.ApiKeyProvider?.Invoke() ?? apiKeyProvider();

            var body = contentFactory.CreateContent(snapshots, out var recordsCount);
            var recordsLength = body.Length - sizeof(int);

            var response = await sender.FireAndForgetAsync(stream, apiKey, body, timeout, cancellationToken)
                .ConfigureAwait(false);

            var responseClass = classifier.Classify(response);

            LogSendingResult(response, responseClass, recordsCount, recordsLength, stream, sw.Elapsed);

            if (responseClass == GateResponseClass.Success)
            {
                state.Statistics.ReportSuccessfulSending(recordsCount, recordsLength);
                RequestGarbageCollection(snapshots);
                return true;
            }

            if (responseClass == GateResponseClass.DefinitiveFailure)
            {
                state.Statistics.ReportSendingFailure(recordsCount, recordsLength);
                RequestGarbageCollection(snapshots);
            }

            return false;
        }

        private void LogSendingResult(
            Response response, 
            GateResponseClass responseClass, 
            int recordsCount, 
            long bytesCount, 
            string stream, 
            TimeSpan elapsed)
        {
            if (responseClass == GateResponseClass.Success)
            {
                log.Info(
                    "Sending {RecordsCount} records of size {RecordsSize} to stream {StreamName} succeeded in {ElapsedTime}",
                    recordsCount,
                    bytesCount,
                    stream,
                    elapsed);
            }
            else
            {
                log.Warn(
                    "Sending {RecordsCount} records of size {RecordsSize} to stream {StreamName} failed after {ElapsedTime} with code {Code}",
                    recordsCount,
                    bytesCount,
                    stream,
                    elapsed,
                    response.Code);
            }
        }
    }
}