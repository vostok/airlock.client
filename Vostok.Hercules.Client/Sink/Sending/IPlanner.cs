using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal interface IPlanner
    {
        Task WaitForNextSendAsync(StreamSendResult result, CancellationToken cancellationToken);
    }
}