using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal interface IJobHandler
    {
        void HandleCompletedJob([NotNull] Task completedJobTask, [NotNull] SchedulerState state);
    }
}