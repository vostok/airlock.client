using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Hercules.Client.Sink.Scheduler.Helpers;

namespace Vostok.Hercules.Client.Sink.Scheduler
{
    internal class Scheduler : IScheduler
    {
        private readonly IStateSynchronizer synchronizer;
        private readonly IFlowController controller;
        private readonly IJobWaiter jobWaiter;
        private readonly IJobHandler jobHandler;

        public Scheduler(
            [NotNull] IStateSynchronizer synchronizer,
            [NotNull] IFlowController controller,
            [NotNull] IJobWaiter jobWaiter,
            [NotNull] IJobHandler jobHandler)
        {
            this.synchronizer = synchronizer;
            this.controller = controller;
            this.jobWaiter = jobWaiter;
            this.jobHandler = jobHandler;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var cancellationTaskSource = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(() => cancellationTaskSource.TrySetCanceled()))
            {
                var state = new SchedulerState(cancellationTaskSource.Task, cancellationToken);

                while (controller.ShouldStillOperateOn(state))
                {
                    synchronizer.Synchronize(state);

                    var completedJobTask = await jobWaiter.WaitForNextCompletedJob(state).ConfigureAwait(false);

                    if (controller.ShouldStillOperateOn(state))
                        jobHandler.HandleCompletedJob(completedJobTask, state);
                    else break;
                }

                foreach (var sendingJob in state.SendingJobs)
                    await sendingJob.SilentlyContinue().ConfigureAwait(false);

                synchronizer.Synchronize(state);

                foreach (var job in state.AllJobs.Values.Where(job => job.IsHealthy))
                    await job.SendAsync(CancellationToken.None).SilentlyContinue().ConfigureAwait(false);
            }
        }
    }
}