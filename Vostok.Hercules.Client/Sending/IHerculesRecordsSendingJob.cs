using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sending
{
    internal interface IHerculesRecordsSendingJob
    {
        long SentRecordsCount { get; }
        long LostRecordsCount { get; }
        Task WaitNextOccurrenceAsync();
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}