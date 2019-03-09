using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal interface IStreamSender
    {
        Task<SendResult> SendAsync(TimeSpan timeout, CancellationToken cancellationToken);
    }
}