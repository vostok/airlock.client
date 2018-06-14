using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Airlock.Client
{
    internal interface IAirlockRecordsSendingJobSchedule
    {
        Task WaitNextOccurrenceAsync(CancellationToken cancellationToken = default);
    }
}