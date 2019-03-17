using Vostok.Hercules.Client.Sink.Job;
using Vostok.Hercules.Client.Sink.State;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal class StateSynchronizer : IStateSynchronizer
    {
        private readonly IStreamStatesProvider statesProvider;
        private readonly IStreamJobFactory jobFactory;
        private readonly IJobLauncher jobLauncher;

        public StateSynchronizer(
            IStreamStatesProvider statesProvider,
            IStreamJobFactory jobFactory,
            IJobLauncher jobLauncher)
        {
            this.statesProvider = statesProvider;
            this.jobFactory = jobFactory;
            this.jobLauncher = jobLauncher;
        }

        public void Synchronize(SchedulerState state)
        {
            foreach (var streamState in statesProvider.GetStates())
            {
                if (state.AllJobs.ContainsKey(streamState.Name))
                    continue;

                var newJob = state.AllJobs[streamState.Name] = jobFactory.CreateJob(streamState);

                jobLauncher.LaunchWaitJob(newJob, state);
            }
        }
    }
}