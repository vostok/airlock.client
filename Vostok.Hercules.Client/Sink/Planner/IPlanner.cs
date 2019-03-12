using System.Threading;
using System.Threading.Tasks;
using Vostok.Hercules.Client.Sink.Sending;

namespace Vostok.Hercules.Client.Sink.Planner
{
    internal interface IPlanner
    {
        Task WaitForNextSendAsync(StreamSendResult result, CancellationToken cancellationToken);
    }
}