using Vostok.Hercules.Client.Sink.StreamState;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal interface IStreamSenderFactory
    {
        IStreamSender Create(IStreamState state);
    }
}