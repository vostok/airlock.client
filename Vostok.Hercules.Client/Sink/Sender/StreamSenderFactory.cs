using System;
using System.Collections.Generic;
using Vostok.Commons.Collections;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Internal;
using Vostok.Hercules.Client.Sink.Analyzer;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.State;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Sender
{
    internal class StreamSenderFactory : IStreamSenderFactory
    {
        private static readonly IReadOnlyDictionary<LogLevel, LogLevel> SuppressVerboseLoggingLevelsTransformation = new Dictionary<LogLevel, LogLevel>
        {
            [LogLevel.Error] = LogLevel.Warn
        };

        private readonly Func<string> apiKeyProvider;
        private readonly IBufferSnapshotBatcher snapshotBatcher;
        private readonly IRequestContentFactory contentFactory;
        private readonly IGateRequestSender requestSender;
        private readonly IResponseAnalyzer responseAnalyzer;
        private readonly IStatusAnalyzer statusAnalyzer;
        private readonly ILog log;

        public StreamSenderFactory(HerculesSinkSettings settings, ILog log)
        {
            this.log = log;

            var bufferPool = new BufferPool(settings.MaximumBatchSize * 2, settings.MaxParallelStreams * 8);
            apiKeyProvider = settings.ApiKeyProvider;
            contentFactory = new RequestContentFactory(bufferPool);
            snapshotBatcher = new BufferSnapshotBatcher(settings.MaximumBatchSize);

            requestSender = new GateRequestSender(
                settings.Cluster,
                settings.SuppressVerboseLogging
                    ? log.WithMinimumLevel(LogLevel.Warn).WithLevelsTransformation(SuppressVerboseLoggingLevelsTransformation)
                    : log,
                bufferPool,
                settings.AdditionalSetup);

            responseAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Stream);
            statusAnalyzer = new StatusAnalyzer();
        }

        public IStreamSender Create(IStreamState state) =>
            new StreamSender(
                apiKeyProvider,
                state,
                snapshotBatcher,
                contentFactory,
                requestSender,
                responseAnalyzer,
                statusAnalyzer,
                log.ForContext(state.Name));
    }
}