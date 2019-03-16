using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Sender
{
    internal interface IStreamSender
    {
        Task<StreamSendResult> SendAsync(TimeSpan perRequestTimeout, CancellationToken cancellationToken);
    }
}