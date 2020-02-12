using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Binary;
using Vostok.Commons.Collections;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Serialization.Readers;
using Vostok.Hercules.Client.Serialization.Writers;
using Vostok.Logging.Abstractions;
using BinaryBufferReader = Vostok.Commons.Binary.BinaryBufferReader;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesTimelineClient<T> : IHerculesTimelineClient<T>
    {
        private readonly Func<IBinaryBufferReader, IHerculesEventBuilder<T>> eventBuilderProvider;
        private readonly ResponseAnalyzer responseAnalyzer;
        private readonly BufferPool bufferPool;
        private readonly IClusterClient client;
        private readonly ILog log;

        public HerculesTimelineClient([NotNull] HerculesTimelineClientSettings<T> settings, [CanBeNull] ILog log)
        {
            this.log = log = (log ?? LogProvider.Get()).ForContext<HerculesTimelineClient>();

            bufferPool = new BufferPool(settings.MaxPooledBufferSize, settings.MaxPooledBuffersPerBucket);

            client = ClusterClientFactory.Create(
                settings.Cluster,
                log,
                Constants.ServiceNames.TimelineApi,
                config =>
                {
                    config.SetupUniversalTransport(
                        new UniversalTransportSettings
                        {
                            BufferFactory = bufferPool.Rent
                        });
                    config.AddRequestTransform(new ApiKeyRequestTransform(settings.ApiKeyProvider));
                    settings.AdditionalSetup?.Invoke(config);
                });

            responseAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Timeline);
            eventBuilderProvider = settings.EventBuilderProvider;
        }

        /// <inheritdoc />
        public async Task<ReadTimelineResult<T>> ReadAsync(ReadTimelineQuery query, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = new RequestUrlBuilder("timeline/read")
                    {
                        {Constants.QueryParameters.Timeline, query.Name},
                        {Constants.QueryParameters.Limit, query.Limit},
                        {Constants.QueryParameters.ClientShard, query.ClientShard},
                        {Constants.QueryParameters.ClientShardCount, query.ClientShardCount},
                        {"from", EpochHelper.ToUnixTimeUtcTicks(query.From.UtcDateTime)},
                        {"to", EpochHelper.ToUnixTimeUtcTicks(query.To.UtcDateTime)}
                    }
                    .Build();

                var body = CreateRequestBody(query.Coordinates ?? TimelineCoordinates.Empty);

                var request = Request
                    .Post(url)
                    .WithContentTypeHeader(Constants.ContentTypes.OctetStream)
                    .WithContent(body);

                var result = await client
                    .SendAsync(request, timeout, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    var operationStatus = responseAnalyzer.Analyze(result.Response, out var errorMessage);
                    if (operationStatus != HerculesStatus.Success)
                        return new ReadTimelineResult<T>(operationStatus, null, errorMessage);

                    return new ReadTimelineResult<T>(operationStatus, ParseResponseBody(result.Response));
                }
                finally
                {
                    if (result.Response.HasContent)
                        bufferPool.Return(result.Response.Content.Buffer);
                }
            }
            catch (Exception error)
            {
                log.Error(error);

                return new ReadTimelineResult<T>(HerculesStatus.UnknownError, null, error.Message);
            }
        }

        private static ArraySegment<byte> CreateRequestBody([NotNull] TimelineCoordinates coordinates)
        {
            var writer = new BinaryBufferWriter(sizeof(int) + coordinates.Positions.Length * (sizeof(int) + sizeof(long) + 24))
            {
                Endianness = Endianness.Big
            };

            TimelineCoordinatesWriter.Write(coordinates, writer);

            return writer.FilledSegment;
        }

        private ReadTimelinePayload<T> ParseResponseBody([NotNull] Response response)
        {
            var reader = new BinaryBufferReader(response.Content.Buffer, response.Content.Offset)
            {
                Endianness = Endianness.Big
            };

            var coordinates = TimelineCoordinatesReader.Read(reader);

            var events = EventsBinaryReader.Read(response.Content.Buffer, reader.Position, eventBuilderProvider, log);

            return new ReadTimelinePayload<T>(events, coordinates);
        }
    }
}