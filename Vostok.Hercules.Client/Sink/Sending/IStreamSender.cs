using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal interface IStreamSender
    {
        Task<StreamSendResult> SendAsync(TimeSpan timeout, CancellationToken cancellationToken);
    }
}