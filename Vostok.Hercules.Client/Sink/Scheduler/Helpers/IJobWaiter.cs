using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal interface IJobWaiter
    {
        Task<Task> WaitForNextCompletedJob([NotNull] SchedulerState state);
    }
}