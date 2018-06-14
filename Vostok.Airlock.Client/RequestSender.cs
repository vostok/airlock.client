using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Airlock.Client
{
    internal class RequestSender : IRequestSender
    {
        public Task<bool> SendAsync(string stream, ArraySegment<byte> message, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}