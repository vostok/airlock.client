﻿using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Binary;
using Vostok.Commons.Collections;
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
    public class HerculesStreamClient : IHerculesStreamClient
    {
        private const int MaxPooledBufferSize = 16 * 1024 * 1024;
        private const int MaxPooledBuffersPerBucket = 8;

        private readonly ResponseAnalyzer responseAnalyzer;
        private readonly BufferPool bufferPool;
        private readonly IClusterClient client;
        private readonly ILog log;

        public HerculesStreamClient([NotNull] HerculesStreamClientSettings settings, [CanBeNull] ILog log)
        {
            this.log = log = (log ?? LogProvider.Get()).ForContext<HerculesStreamClient>();

            bufferPool = new BufferPool(MaxPooledBufferSize, MaxPooledBuffersPerBucket);

            client = ClusterClientFactory.Create(
                settings.Cluster,
                log,
                Constants.ServiceNames.StreamApi,
                config =>
                {
                    config.SetupUniversalTransport(new UniversalTransportSettings
                    {
                        BufferFactory = bufferPool.Rent
                    });
                    config.AddRequestTransform(new ApiKeyRequestTransform(settings.ApiKeyProvider));
                    settings.AdditionalSetup?.Invoke(config);
                });

            responseAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Stream);
        }

        /// <inheritdoc />
        public async Task<ReadStreamResult> ReadAsync(ReadStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = new RequestUrlBuilder("stream/read")
                    {
                        {Constants.QueryParameters.Stream, query.Name},
                        {Constants.QueryParameters.Limit, query.Limit},
                        {Constants.QueryParameters.ClientShard, query.ClientShard},
                        {Constants.QueryParameters.ClientShardCount, query.ClientShardCount}
                    }
                    .Build();

                var body = CreateRequestBody(query.Coordinates ?? StreamCoordinates.Empty);

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
                        return new ReadStreamResult(operationStatus, null, errorMessage);

                    return new ReadStreamResult(operationStatus, ParseResponseBody(result.Response));
                }
                finally
                {
                    if (result.Response.HasContent)
                        bufferPool.Return(result.Response.Content.Buffer);
                }
            }
            catch (Exception error)
            {
                log.Warn(error);
                return new ReadStreamResult(HerculesStatus.UnknownError, null, error.Message);
            }
        }

        private static ArraySegment<byte> CreateRequestBody([NotNull] StreamCoordinates coordinates)
        {
            var writer = new BinaryBufferWriter(sizeof(int) + coordinates.Positions.Length * (sizeof(int) + sizeof(long)))
            {
                Endianness = Endianness.Big
            };

            StreamCoordinatesWriter.Write(coordinates, writer);

            return writer.FilledSegment;
        }

        private static ReadStreamPayload ParseResponseBody([NotNull] Response response)
        {
            var reader = new BinaryBufferReader(response.Content.Buffer, response.Content.Offset)
            {
                Endianness = Endianness.Big
            };

            var coordinates = StreamCoordinatesReader.Read(reader);

            var events = reader.ReadArray(BinaryEventReader.ReadEvent);

            return new ReadStreamPayload(events, coordinates);
        }
    }
}
