using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.State;

namespace Vostok.Hercules.Client.Sink.Job
{
    internal interface IStreamJobFactory
    {
        [NotNull]
        IStreamJob CreateJob([NotNull] IStreamState state);
    }
}