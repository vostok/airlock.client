using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hercules.Client.Gate
{
    internal interface IGateRequestSender
    {
        Task<Response> SendAsync(
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] Content content,
            TimeSpan timeout,
            CancellationToken cancellationToken);

        Task<Response> FireAndForgetAsync(
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] Content content,
            TimeSpan timeout,
            CancellationToken cancellationToken);
    }
}