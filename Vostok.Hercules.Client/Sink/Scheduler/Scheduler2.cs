using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.Scheduler.Helpers;
using Vostok.Hercules.Client.Utilities;

namespace Vostok.Hercules.Client.Sink.Scheduler
{
    internal class Scheduler2 : IScheduler
    {
        private readonly IStateSynchronizer synchronizer;
        private readonly IFlowController controller;
        private readonly IJobWaiter jobWaiter;
        private readonly IJobHandler jobHandler;

        public Scheduler2(
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
            using (cancellationToken.CreateTask(out var cancellationTask))
            {
                var state = new SchedulerState(cancellationTask, cancellationToken);

                while (controller.ShouldStillOperateOn(state))
                {
                    synchronizer.Synchronize(state);

                    var completedJobTask = await jobWaiter.WaitForNextCompletedJob(state).ConfigureAwait(false);

                    if (controller.ShouldStillOperateOn(state))
                        jobHandler.HandleCompletedJob(completedJobTask, state);
                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}