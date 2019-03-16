using System;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.StreamState;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal class StreamSenderFactory : IStreamSenderFactory
    {
        private readonly Func<string> apiKeyProvider;
        private readonly IBufferSnapshotBatcher batcher;
        private readonly IRequestContentFactory contentFactory;
        private readonly IGateRequestSender requestSender;
        private readonly IGateResponseClassifier responseClassifier;
        private readonly ILog log;


        public StreamSenderFactory(
            Func<string> apiKeyProvider,
            IBufferSnapshotBatcher batcher,
            IRequestContentFactory contentFactory,
            IGateRequestSender requestSender,
            ILog log)
        {
            this.apiKeyProvider = apiKeyProvider;
            this.batcher = batcher;
            this.contentFactory = contentFactory;
            this.requestSender = requestSender;
            this.log = log;

            responseClassifier = new GateResponseClassifier(new ResponseAnalyzer(ResponseAnalysisContext.Stream));
        }

        public IStreamSender Create(IStreamState state) =>
            new StreamSender(apiKeyProvider, state, batcher, contentFactory, requestSender, responseClassifier, log);
    }
}