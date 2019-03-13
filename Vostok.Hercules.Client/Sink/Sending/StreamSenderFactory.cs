using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.StreamState;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal class StreamSenderFactory : IStreamSenderFactory
    {
        private readonly IBufferSnapshotBatcher batcher;
        private readonly IRequestContentFactory contentFactory;
        private readonly IRequestSender requestSender;
        private readonly ILog log;

        public StreamSenderFactory(
            IBufferSnapshotBatcher batcher,
            IRequestContentFactory contentFactory,
            IRequestSender requestSender,
            ILog log)
        {
            this.batcher = batcher;
            this.contentFactory = contentFactory;
            this.requestSender = requestSender;
            this.log = log;
        }

        public IStreamSender Create(IStreamState state) =>
            new StreamSender(state, batcher, contentFactory, requestSender, log);
    }
}