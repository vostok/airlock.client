using System;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Sink.Analyzer;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.State;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Sender
{
    internal class StreamSenderFactory : IStreamSenderFactory
    {
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

            apiKeyProvider = settings.ApiKeyProvider;
            contentFactory = new RequestContentFactory();
            snapshotBatcher = new BufferSnapshotBatcher(settings.MaximumBatchSize);

            requestSender = new GateRequestSender(
                settings.Cluster, 
                settings.SuppressVerboseLogging ? log.WithMinimumLevel(LogLevel.Warn) : log,
                settings.AdditionalSetup);

            responseAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Stream);
            statusAnalyzer = new StatusAnalyzer();
        }

        public IStreamSender Create(IStreamState state) =>
            new StreamSender(apiKeyProvider, state, snapshotBatcher, contentFactory, 
                requestSender, responseAnalyzer, statusAnalyzer, log.ForContext(state.Name));
    }
}