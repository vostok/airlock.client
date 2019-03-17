using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal interface IStateSynchronizer
    {
        void Synchronize([NotNull] SchedulerState state);
    }
}