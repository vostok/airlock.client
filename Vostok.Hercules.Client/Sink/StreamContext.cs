using Vostok.Hercules.Client.Sink.Sending;

namespace Vostok.Hercules.Client.Sink
{
    internal struct StreamContext
    {
        public StreamContext(ISchedulingStreamSender sender, IStreamState state)
        {
            Sender = sender;
            State = state;
        }

        public ISchedulingStreamSender Sender { get; }
        public IStreamState State { get; }
    }
}