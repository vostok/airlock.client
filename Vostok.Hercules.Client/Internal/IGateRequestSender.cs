using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Hercules.Client.Abstractions.Results;

namespace Vostok.Hercules.Client.Internal
{
    internal interface IGateRequestSender
    {
        Task<InsertEventsResult> SendAsync(
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] ValueDisposable<Content> content,
            TimeSpan timeout,
            CancellationToken cancellationToken);

        Task<InsertEventsResult> FireAndForgetAsync(
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] ValueDisposable<Content> content,
            TimeSpan timeout,
            CancellationToken cancellationToken);
    }
}