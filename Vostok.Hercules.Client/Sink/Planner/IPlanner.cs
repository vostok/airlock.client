using System.Threading;
using System.Threading.Tasks;
using Vostok.Hercules.Client.Sink.Sender;

namespace Vostok.Hercules.Client.Sink.Planner
{
    internal interface IPlanner
    {
        Task WaitForNextSendAsync(StreamSendResult result, CancellationToken cancellationToken);
    }
}