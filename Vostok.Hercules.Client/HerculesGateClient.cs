using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Binary;
using Vostok.Commons.Collections;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Serialization.Builders;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesGateClient : IHerculesGateClient
    {
        private const int InitialBodyBufferSize = 4096;

        private readonly UnboundedObjectPool<BinaryBufferWriter> BufferPool
            = new UnboundedObjectPool<BinaryBufferWriter>(() => new BinaryBufferWriter(InitialBodyBufferSize) {Endianness = Endianness.Big});

        private readonly HerculesGateClientSettings settings;
        private readonly ResponseAnalyzer responseAnalyzer;
        private readonly IGateRequestSender sender;
        private readonly ILog log;

        /// <inheritdoc />
        public HerculesGateClient(HerculesGateClientSettings settings, ILog log)
        {
            this.settings = settings;
            this.log = log = (log ?? LogProvider.Get()).ForContext<HerculesGateClient>();

            sender = new GateRequestSender(settings.Cluster, log, settings.AdditionalSetup);
            responseAnalyzer = new ResponseAnalyzer();
        }

        /// <inheritdoc />
        public async Task<InsertEventsResult> InsertAsync(
            InsertEventsQuery query,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using (BufferPool.Acquire(out var buffer))
                {
                    var content = CreateContent(query, buffer);

                    var response = await sender
                        .SendAsync(query.Stream, settings.ApiKeyProvider(), content, timeout, cancellationToken)
                        .ConfigureAwait(false);

                    var operationStatus = responseAnalyzer.Analyze(response, out var errorMessage);

                    return new InsertEventsResult(operationStatus, errorMessage);
                }
            }
            catch (Exception error)
            {
                log.Warn(error);
                return new InsertEventsResult(HerculesStatus.UnknownError);
            }
        }

        private static Content CreateContent(InsertEventsQuery query, BinaryBufferWriter buffer)
        {
            buffer.Write(query.Events.Count);

            foreach (var @event in query.Events)
            {
                using (var eventBuilder = new BinaryEventBuilder(buffer, () => PreciseDateTime.UtcNow, Constants.EventProtocolVersion))
                {
                    eventBuilder
                        .SetTimestamp(@event.Timestamp)
                        .AddTags(@event.Tags);
                }
            }

            return new Content(buffer.FilledSegment);
        }
    }
}
