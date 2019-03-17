using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Sink.Scheduler
{
    internal interface IScheduler
    {
        [NotNull]
        [ItemNotNull]
        Task<SchedulerState> RunAsync(CancellationToken cancellationToken);

        Task CleanupAsync([NotNull] SchedulerState state);
    }
}