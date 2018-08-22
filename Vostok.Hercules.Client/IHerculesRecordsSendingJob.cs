using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client
{
    internal interface IHerculesRecordsSendingJob
    {
        int SentRecordsCount { get; }
        int LostRecordsCount { get; }
        Task WaitNextOccurrenceAsync();
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}