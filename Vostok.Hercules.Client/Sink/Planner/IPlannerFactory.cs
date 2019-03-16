using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.State;

namespace Vostok.Hercules.Client.Sink.Planner
{
    internal interface IPlannerFactory
    {
        [NotNull]
        IPlanner Create([NotNull] IStreamState state);
    }
}