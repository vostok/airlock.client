using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Helpers.Disposable;

namespace Vostok.Hercules.Client.Internal
{
    internal interface IGateRequestSender
    {
        Task<Response> SendAsync(
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] ValueDisposable<Content> content,
            TimeSpan timeout,
            CancellationToken cancellationToken);

        Task<Response> FireAndForgetAsync(
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] ValueDisposable<Content> content,
            TimeSpan timeout,
            CancellationToken cancellationToken);
    }
}