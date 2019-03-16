using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hercules.Client.Gate
{
    internal interface IGateRequestSender
    {
        Task<RequestSendingResult> SendAsync(
            string stream,
            Content content,
            TimeSpan timeout,
            Func<string> apiKeyProvider = null,
            CancellationToken cancellationToken = default);

        Task<RequestSendingResult> FireAndForgetAsync(
            string stream,
            CompositeContent content,
            TimeSpan timeout,
            Func<string> apiKeyProvider = null,
            CancellationToken cancellationToken = default);
    }
}