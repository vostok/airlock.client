using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.Job;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal interface IJobLauncher
    {
        void LaunchWaitJob([NotNull] IStreamJob job, [NotNull] SchedulerState state);

        void LaunchSendJob([NotNull] IStreamJob job, [NotNull] SchedulerState state);
    }
}