using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Sink.Planning;
using Vostok.Hercules.Client.Sink.Sender;
using Vostok.Hercules.Client.Sink.State;
using Vostok.Hercules.Client.Utilities;
using WaitingJob = System.Threading.Tasks.Task<string>;
using RunningJob = System.Threading.Tasks.Task<(string stream, Vostok.Hercules.Client.Sink.Sender.StreamSendResult result)>;

namespace Vostok.Hercules.Client.Sink.Scheduler
{
    internal class Scheduler : IScheduler
    {
        private readonly Dictionary<string, (IStreamSender sender, IPlanner planner)> senders = new Dictionary<string, (IStreamSender, IPlanner)>();

        private readonly IStreamSenderFactory senderFactory;
        private readonly IPlannerFactory plannerFactory;
        private readonly WeakReference<IHerculesSink> sinkReference;
        private readonly ConcurrentDictionary<string, Lazy<IStreamState>> states;
        private readonly HerculesSinkSettings settings;
        private readonly List<RunningJob> runningJobs = new List<RunningJob>();
        private readonly List<WaitingJob> waitingJobs = new List<WaitingJob>();

        public Scheduler(
            IHerculesSink sink,
            ConcurrentDictionary<string, Lazy<IStreamState>> states,
            HerculesSinkSettings settings,
            IStreamSenderFactory senderFactory,
            IPlannerFactory plannerFactory)
        {
            sinkReference = new WeakReference<IHerculesSink>(sink);
            this.states = states;
            this.settings = settings;
            this.senderFactory = senderFactory;
            this.plannerFactory = plannerFactory;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using (cancellationToken.CreateTask(out var cancellationTask))
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (SinkIsCollectedByGC() && AllBuffersIsEmpty())
                        return;

                    LookForNewStreams(cancellationToken);

                    var task = await WaitForNextTaskAsync(cancellationTask).ConfigureAwait(false);

                    switch (task)
                    {
                        case RunningJob runningJob:
                            var (stream, result) = runningJob.GetAwaiter().GetResult();
                            runningJobs.Remove(runningJob);
                            waitingJobs.Add(CreateWaiterAsync(stream, result, cancellationToken));
                            break;
                        case WaitingJob waitingJob:
                            var nextStream = waitingJob.GetAwaiter().GetResult();
                            waitingJobs.Remove(waitingJob);
                            runningJobs.Add(RunJobAsync(nextStream, cancellationToken));
                            break;
                        default:
                            task.GetAwaiter().GetResult();
                            break;
                    }
                }
        }

        public void Dispose()
        {
            CompleteAllRunningJobs();
            foreach (var sender in senders)
                sender.Value.sender
                    .SendAsync(settings.RequestTimeout, CancellationToken.None)
                    .SilentlyContinue()
                    .GetAwaiter()
                    .GetResult();
        }

        private bool AllBuffersIsEmpty()
        {
            return states
                .Where(x => x.Value.IsValueCreated)
                .All(x => x.Value.Value.Statistics.EstimateStoredSize() == 0);
        }

        private void LookForNewStreams(CancellationToken cancellationToken)
        {
            foreach (var pair in states.Select(x => x))
            {
                if (!pair.Value.IsValueCreated)
                    continue;

                var stream = pair.Key;

                if (senders.ContainsKey(stream))
                    continue;

                var streamState = pair.Value.Value;

                var sender = senderFactory.Create(streamState);
                var planner = plannerFactory.Create(streamState);

                senders[pair.Key] = (sender, planner);
                waitingJobs.Add(CreateWaiterAsync(stream, StreamSendResult.Success, cancellationToken));
            }
        }

        private Task<Task> WaitForNextTaskAsync(Task cancellationTask)
        {
            var tasks = new List<Task>();

            tasks.AddRange(runningJobs);

            if (runningJobs.Count < settings.MaxParallelStreams)
                tasks.AddRange(waitingJobs);

            if (tasks.Count == 0)
                tasks.Add(Task.Delay(settings.SendPeriod));

            tasks.Add(cancellationTask);

            return Task.WhenAny(tasks);
        }

        private async WaitingJob CreateWaiterAsync(string stream, StreamSendResult result, CancellationToken cancellationToken)
        {
            var (_, planner) = senders[stream];
            await planner.WaitForNextSendAsync(result, cancellationToken).ConfigureAwait(false);
            return stream;
        }

        private async RunningJob RunJobAsync(string stream, CancellationToken cancellationToken)
        {
            var (sender, _) = senders[stream];
            states[stream].Value.SendSignal.Reset();
            var result = await sender.SendAsync(settings.RequestTimeout, cancellationToken).ConfigureAwait(false);
            return (stream, result);
        }

        private void CompleteAllRunningJobs()
        {
            foreach (var job in runningJobs)
                job.SilentlyContinue().GetAwaiter().GetResult();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SinkIsCollectedByGC()
        {
            return !sinkReference.TryGetTarget(out _);
        }
    }
}