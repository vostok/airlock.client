using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Airlock.Client
{
    internal interface IRequestSender
    {
        Task<bool> SendAsync(string stream, ArraySegment<byte> message, CancellationToken cancellationToken = default);
    }
}