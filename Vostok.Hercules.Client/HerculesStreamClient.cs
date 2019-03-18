using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Binary;
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
        private readonly HerculesStreamClientSettings settings;
        private readonly ResponseAnalyzer responseAnalyzer;
        private readonly IClusterClient client;
        private readonly ILog log;

        public HerculesStreamClient([NotNull] HerculesStreamClientSettings settings, [CanBeNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.log = log = (log ?? LogProvider.Get()).ForContext<HerculesStreamClient>();

            client = ClusterClientFactory.Create(settings.Cluster, log, Constants.ServiceNames.StreamApi, settings.AdditionalSetup);

            responseAnalyzer = new ResponseAnalyzer();
        }

        /// <inheritdoc />
        public async Task<ReadStreamResult> ReadAsync(ReadStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
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
                    .WithHeader(Constants.HeaderNames.ApiKey, settings.ApiKeyProvider())
                    .WithContent(body);

                var result = await client
                    .SendAsync(request, timeout, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var operationStatus = responseAnalyzer.Analyze(result.Response, out var errorMessage);
                if (operationStatus != HerculesStatus.Success)
                    return new ReadStreamResult(operationStatus, null, errorMessage);

                return new ReadStreamResult(HerculesStatus.Success, ParseResponseBody(result.Response));
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
