using Vostok.Hercules.Client.Gateway;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.StreamState;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal interface IStreamSenderFactory
    {
        (IStreamSender, IPlanner) Create(IStreamState state);
    }

    internal class StreamSenderFactory : IStreamSenderFactory
    {
        private readonly HerculesSinkSettings settings;
        private readonly IBufferSnapshotBatcher batcher;
        private readonly IRequestContentFactory contentFactory;
        private readonly IRequestSender requestSender;
        private readonly ILog log;

        public StreamSenderFactory(
            HerculesSinkSettings settings,
            IBufferSnapshotBatcher batcher,
            IRequestContentFactory contentFactory,
            IRequestSender requestSender,
            ILog log)
        {
            this.settings = settings;
            this.batcher = batcher;
            this.contentFactory = contentFactory;
            this.requestSender = requestSender;
            this.log = log;
        }

        public (IStreamSender, IPlanner) Create(IStreamState state)
        {
            var sender = new StreamSender(state, batcher, contentFactory, requestSender, log);
            var planner = new Planner(state.SendSignal, settings.SendPeriod, settings.SendPeriodCap);

            return (sender, planner);
        }
    }
}