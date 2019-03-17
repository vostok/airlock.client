using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Hercules.Client.Sink.Job;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal class SchedulerState
    {
        public SchedulerState(Task cancellationTask, CancellationToken cancellationToken)
        {
            CancellationTask = cancellationTask;
            CancellationToken = cancellationToken;
        }

        public Task CancellationTask { get; }

        public CancellationToken CancellationToken { get; }

        public List<Task> SendingJobs { get; } = new List<Task>();

        public List<Task> WaitingJobs { get; } = new List<Task>();

        public ConcurrentDictionary<string, IStreamJob> AllJobs = new ConcurrentDictionary<string, IStreamJob>();
    }
}