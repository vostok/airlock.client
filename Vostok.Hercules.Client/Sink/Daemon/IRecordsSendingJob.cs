using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Daemon
{
    internal interface IRecordsSendingJob
    {
        Task WaitNextOccurrenceAsync();
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}