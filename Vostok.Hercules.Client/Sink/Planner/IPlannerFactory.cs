using Vostok.Hercules.Client.Sink.StreamState;

namespace Vostok.Hercules.Client.Sink.Planner
{
    internal interface IPlannerFactory
    {
        IPlanner Create(IStreamState state);
    }
}