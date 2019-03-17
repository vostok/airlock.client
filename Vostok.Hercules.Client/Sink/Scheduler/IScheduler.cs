using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Scheduler
{
    internal interface IScheduler
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}