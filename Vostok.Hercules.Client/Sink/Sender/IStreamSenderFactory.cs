using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.State;

namespace Vostok.Hercules.Client.Sink.Sender
{
    internal interface IStreamSenderFactory
    {
        [NotNull]
        IStreamSender Create([NotNull] IStreamState state);
    }
}