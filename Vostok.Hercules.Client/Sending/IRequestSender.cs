using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hercules.Client.Sending
{
    internal interface IRequestSender
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