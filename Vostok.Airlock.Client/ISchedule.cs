using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Airlock.Client
{
    internal interface ISchedule
    {
        Task WaitNextOccurrenceAsync(CancellationToken cancellationToken = default);
    }
}