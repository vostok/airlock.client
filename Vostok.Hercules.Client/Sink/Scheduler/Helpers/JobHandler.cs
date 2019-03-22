using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal class JobHandler : IJobHandler
    {
        private readonly IJobLauncher launcher;

        public JobHandler(IJobLauncher launcher)
        {
            this.launcher = launcher;
        }

        public void HandleCompletedJob(Task completedJobTask, SchedulerState state)
        {
            if (completedJobTask is Task<SendingJobResult> sendingJobTask)
            {
                state.SendingJobs.Remove(completedJobTask);

                launcher.LaunchWaitJob(sendingJobTask.GetAwaiter().GetResult().Job, state);
            }

            if (completedJobTask is Task<WaitingJobResult> waitingJobTask)
            {
                state.WaitingJobs.Remove(completedJobTask);

                launcher.LaunchSendJob(waitingJobTask.GetAwaiter().GetResult().Job, state);
            }

            completedJobTask.GetAwaiter().GetResult();
        }
    }
}