using Vostok.Hercules.Client.Sink.State;

namespace Vostok.Hercules.Client.Sink.Planner
{
    internal interface IPlannerFactory
    {
        IPlanner Create(IStreamState state);
    }
}