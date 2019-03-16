using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Job
{
    internal interface IStreamJob
    {
        Task SendAsync(CancellationToken token);

        Task WaitForNextSendAsync(CancellationToken token);
    }
}
