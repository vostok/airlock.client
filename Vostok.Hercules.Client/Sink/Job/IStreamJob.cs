using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Job
{
    internal interface IStreamJob
    {
        bool IsHealthy { get; }

        Task SendAsync(CancellationToken cancellationToken);

        Task WaitForNextSendAsync(CancellationToken cancellationToken);
    }
}