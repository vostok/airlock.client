using Vostok.Hercules.Client.Sink.Job;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal class JobLauncher : IJobLauncher
    {
        public void LaunchWaitJob(IStreamJob job, SchedulerState state)
            => state.WaitingJobs.Add(job.WaitForNextSendAsync(state.CancellationToken).ContinueWith(_ => new WaitingJobResult(job)));

        public void LaunchSendJob(IStreamJob job, SchedulerState state)
            => state.SendingJobs.Add(job.SendAsync(state.CancellationToken).ContinueWith(_ => new SendingJobResult(job)));
    }
}