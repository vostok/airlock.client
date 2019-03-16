using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.StreamState;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal interface IStreamSenderFactory
    {
        [NotNull]
        IStreamSender Create([NotNull] IStreamState state);
    }
}