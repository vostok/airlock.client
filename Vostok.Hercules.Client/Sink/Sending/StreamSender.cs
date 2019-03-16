using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.StreamState;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal class StreamSender : IStreamSender
    {
        private readonly Func<string> globalApiKeyProvider;
        private readonly IStreamState streamState;
        private readonly IBufferSnapshotBatcher snapshotBatcher;
        private readonly IRequestContentFactory contentFactory;
        private readonly IGateRequestSender gateRequestSender;
        private readonly IGateResponseClassifier gateResponseClassifier;
        private readonly ILog log;

        public StreamSender(
            [NotNull] Func<string> globalApiKeyProvider,
            [NotNull] IStreamState streamState,
            [NotNull] IBufferSnapshotBatcher snapshotBatcher,
            [NotNull] IRequestContentFactory contentFactory,
            [NotNull] IGateRequestSender gateRequestSender,
            [NotNull] IGateResponseClassifier gateResponseClassifier,
            [NotNull] ILog log)
        {
            this.globalApiKeyProvider = globalApiKeyProvider;
            this.streamState = streamState;
            this.snapshotBatcher = snapshotBatcher;
            this.contentFactory = contentFactory;
            this.gateRequestSender = gateRequestSender;
            this.gateResponseClassifier = gateResponseClassifier;
            this.log = log;
        }

        public async Task<StreamSendResult> SendAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var snapshots = CollectSnapshots();

            var batches = snapshotBatcher.Batch(snapshots);

            var result = StreamSendResult.NothingToSend;

            foreach (var batch in batches)
            {
                cancellationToken.ThrowIfCancellationRequested();

                result = await SendBatchAsync(batch, timeout, cancellationToken).ConfigureAwait(false);

                if (result == StreamSendResult.Failure)
                    return result;
            }

            return result;
        }

        [NotNull]
        private IEnumerable<BufferSnapshot> CollectSnapshots()
            => streamState.BufferPool
                .Select(buffer => buffer.TryMakeSnapshot())
                .Where(snapshot => snapshot != null)
                .Where(snapshot => snapshot.State.RecordsCount > 0);

        [CanBeNull]
        private string ObtainApiKey()
            => streamState.Settings.ApiKeyProvider?.Invoke() ?? globalApiKeyProvider();

        private async Task<StreamSendResult> SendBatchAsync([NotNull] IReadOnlyList<BufferSnapshot> batch, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var watch = Stopwatch.StartNew();

            var body = contentFactory.CreateContent(batch, out var recordsCount, out var recordsSize);

            var response = await gateRequestSender
                .FireAndForgetAsync(streamState.Name, ObtainApiKey(), body, timeout, cancellationToken)
                .ConfigureAwait(false);

            var responseClass = gateResponseClassifier.Classify(response);

            if (responseClass != GateResponseClass.TransientFailure)
            {
                RequestGarbageCollection(batch);

                if (responseClass == GateResponseClass.Success)
                    streamState.Statistics.ReportSuccessfulSending(recordsCount, recordsSize);

                if (responseClass == GateResponseClass.DefinitiveFailure)
                    streamState.Statistics.ReportSendingFailure(recordsCount, recordsSize);
            }

            var result = responseClass == GateResponseClass.Success 
                ? StreamSendResult.Success 
                : StreamSendResult.Failure;

            if (result == StreamSendResult.Success)
                LogBatchSendSuccess(recordsCount, recordsSize, watch.Elapsed);
            else
                LogBatchSendFailure(recordsCount, recordsSize, response.Code, responseClass);

            return result;
        }

        private static void RequestGarbageCollection([NotNull] IEnumerable<BufferSnapshot> snapshots)
        {
            foreach (var snapshot in snapshots)
                snapshot.Source.ReportGarbage(snapshot.State);
        }

        private void LogBatchSendSuccess(int recordsCount, long recordsSize, TimeSpan elapsed)
            => log.Info("Successfully sent {RecordsCount} records of size {RecordsSize} to stream '{StreamName}' in {ElapsedTime}.",
                recordsCount, recordsSize, streamState.Name, elapsed.ToPrettyString());

        private void LogBatchSendFailure(int recordsCount, long recordsSize, ResponseCode code, GateResponseClass responseClass)
        {
            log.Warn(
                "Failed to send {RecordsCount} records of size {RecordsSize} to stream '{StreamName}'. " +
                "Response code = {NumericResponseCode} ({ResponseCode}). Response class = {ResponseClass}",
                recordsCount, recordsSize, streamState.Name, (int) code, code, responseClass);

            if (responseClass == GateResponseClass.DefinitiveFailure)
                log.Warn("Dropped {RecordsCount} records as a result of non-retriable failure.", recordsCount);
        }
    }
}