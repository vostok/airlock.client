using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesStreamClient : IHerculesStreamClient
    {
        private readonly HerculesStreamClient<HerculesEvent> client;

        public HerculesStreamClient([NotNull] HerculesStreamClientSettings settings, [CanBeNull] ILog log) =>
            client = new HerculesStreamClient<HerculesEvent>(settings, log);

        public async Task<ReadStreamResult> ReadAsync(ReadStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await client.ReadAsync(query, timeout, cancellationToken).ConfigureAwait(false);
            return result.FromGenericResult();
        }

        public async Task<ReadStreamIEnumerableResult> ReadIEnumerableAsync(ReadStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await client.ReadIEnumerableAsync(query, timeout, cancellationToken).ConfigureAwait(false);
            return result.FromGenericResult();
        }

        public Task<SeekToEndStreamResult> SeekToEndAsync(SeekToEndStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken()) =>
            client.SeekToEndAsync(query, timeout, cancellationToken);
    }
}