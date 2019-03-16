using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Sink.StreamState
{
    internal interface IStreamStateFactory
    {
        [NotNull]
        IStreamState Create([NotNull] string name);
    }
}