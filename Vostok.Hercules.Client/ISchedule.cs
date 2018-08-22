using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client
{
    internal interface ISchedule
    {
        Task WaitNextOccurrenceAsync(CancellationToken cancellationToken = default);
    }
}