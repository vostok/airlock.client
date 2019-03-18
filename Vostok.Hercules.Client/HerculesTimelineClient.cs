﻿using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Binary;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Serialization.Readers;
using Vostok.Hercules.Client.Serialization.Writers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesTimelineClient : IHerculesTimelineClient
    {
        private readonly ResponseAnalyzer responseAnalyzer;
        private readonly IClusterClient client;
        private readonly ILog log;

        public HerculesTimelineClient([NotNull] HerculesTimelineClientSettings settings, [CanBeNull] ILog log)
        {
            this.log = log = (log ?? LogProvider.Get()).ForContext<HerculesTimelineClient>();

            client = ClusterClientFactory.Create(
                settings.Cluster,
                log,
                Constants.ServiceNames.TimelineApi,
                config =>
                {
                    config.AddRequestTransform(new ApiKeyRequestTransform(settings.ApiKeyProvider));
                    settings.AdditionalSetup?.Invoke(config);
                });

            responseAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Timeline);
        }

        /// <inheritdoc />
        public async Task<ReadTimelineResult> ReadAsync(ReadTimelineQuery query, TimeSpan timeout, CancellationToken cancellationToken = default)
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

                var operationStatus = responseAnalyzer.Analyze(result.Response, out var errorMessage);
                if (operationStatus != HerculesStatus.Success)
                    return new ReadTimelineResult(operationStatus, null, errorMessage);

                return new ReadTimelineResult(operationStatus, ParseResponseBody(result.Response));
            }
            catch (Exception error)
            {
                log.Error(error);

                return new ReadTimelineResult(HerculesStatus.UnknownError, null, error.Message);
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

        private static ReadTimelinePayload ParseResponseBody([NotNull] Response response)
        {
            var reader = new BinaryBufferReader(response.Content.Buffer, response.Content.Offset)
            {
                Endianness = Endianness.Big
            };

            var coordinates = TimelineCoordinatesReader.Read(reader);

            var events = reader.ReadArray(BinaryEventReader.ReadEvent);

            return new ReadTimelinePayload(events, coordinates);
        }
    }
}