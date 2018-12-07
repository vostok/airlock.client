using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client
{
    internal interface IRequestSender
    {
        Task<RequestSendingResult> SendAsync(string stream, ArraySegment<byte> message, TimeSpan timeout, CancellationToken cancellationToken = default);
    }
}