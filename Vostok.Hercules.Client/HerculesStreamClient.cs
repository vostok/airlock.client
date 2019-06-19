using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Serialization.Readers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesStreamClient : IHerculesStreamClient
    {
        private readonly HerculesStreamClient<HerculesEvent> client;

        public HerculesStreamClient([NotNull] HerculesStreamClientSettings settings, [CanBeNull] ILog log)
        {
            var settingsOfT = new HerculesStreamClientSettings<HerculesEvent>(
                settings.Cluster,
                settings.ApiKeyProvider,
                _ => new HerculesEventBuilderGeneric());
            client = new HerculesStreamClient<HerculesEvent>(settingsOfT, log);
        }

        public async Task<ReadStreamResult> ReadAsync(ReadStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await client.ReadAsync(query, timeout, cancellationToken).ConfigureAwait(false);
            return result.FromGenericResult();
        }

        public Task<SeekToEndStreamResult> SeekToEndAsync(SeekToEndStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken()) =>
            client.SeekToEndAsync(query, timeout, cancellationToken);
    }
}