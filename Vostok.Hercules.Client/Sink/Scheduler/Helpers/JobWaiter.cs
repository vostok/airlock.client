using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal class JobWaiter : IJobWaiter
    {
        private readonly TimeSpan defaultWait;
        private readonly int maxJobParallelism;

        public JobWaiter(TimeSpan defaultWait, int maxJobParallelism)
        {
            this.defaultWait = defaultWait;
            this.maxJobParallelism = maxJobParallelism;
        }

        public Task<Task> WaitForNextCompletedJob(SchedulerState state) =>
            Task.WhenAny(GetTasksToWaitFor(state));

        private List<Task> GetTasksToWaitFor(SchedulerState state)
        {
            var result = new List<Task>();

            result.AddRange(state.SendingJobs);

            if (result.Count < maxJobParallelism)
                result.AddRange(state.WaitingJobs);

            if (result.Count == 0)
                result.Add(Task.Delay(defaultWait));

            result.Add(state.CancellationTask);

            return result;
        }
    }
}