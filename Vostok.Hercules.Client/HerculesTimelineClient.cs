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
    public class HerculesTimelineClient : IHerculesTimelineClient
    {
        private readonly HerculesTimelineClient<HerculesEvent> client;

        public HerculesTimelineClient([NotNull] HerculesTimelineClientSettings settings, [CanBeNull] ILog log)
        {
            // CR(iloktionov): Could we get rid of manual settings copying with settings inheritance?

            var settingsOfT = new HerculesTimelineClientSettings<HerculesEvent>(
                settings.Cluster,
                settings.ApiKeyProvider,
                _ => new HerculesEventBuilderGeneric());
            client = new HerculesTimelineClient<HerculesEvent>(settingsOfT, log);
        }

        public async Task<ReadTimelineResult> ReadAsync(ReadTimelineQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await client.ReadAsync(query, timeout, cancellationToken).ConfigureAwait(false);
            return result.FromGenericResult();
        }
    }
}