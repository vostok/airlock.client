using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Sink.State
{
    internal interface IStreamStateFactory
    {
        [NotNull]
        IStreamState Create([NotNull] string name);
    }
}