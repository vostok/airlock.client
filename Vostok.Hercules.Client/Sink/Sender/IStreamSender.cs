using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Sink.Sender
{
    internal interface IStreamSender
    {
        [ItemNotNull]
        Task<StreamSendResult> SendAsync(TimeSpan perRequestTimeout, CancellationToken cancellationToken);
    }
}