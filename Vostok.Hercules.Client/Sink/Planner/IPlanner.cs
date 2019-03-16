using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.Sender;

namespace Vostok.Hercules.Client.Sink.Planner
{
    internal interface IPlanner
    {
        Task WaitForNextSendAsync([NotNull] StreamSendResult lastResult, CancellationToken cancellationToken);
    }
}