using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Serialization.Readers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesStreamClient : IHerculesStreamClient
    {
        private readonly ILog log;
        private readonly IClusterClient client;
        private readonly Func<string> apiKeyProvider;
        private readonly ResponseAnalyzer responseAnalyzer;

        /// <param name="settings">Settings of this <see cref="HerculesStreamClient"/></param>
        /// <param name="log">An <see cref="ILog"/> instance.</param>
        public HerculesStreamClient(HerculesStreamClientSettings settings, ILog log)
        {
            this.log = (log ?? LogProvider.Get()).ForContext<HerculesStreamClient>();

            apiKeyProvider = settings.ApiKeyProvider;

            client = ClusterClientFactory.Create(settings.Cluster, this.log, Constants.ServiceNames.StreamApi, null);

            responseAnalyzer = new ResponseAnalyzer();
        }

        /// <inheritdoc />
        public async Task<ReadStreamResult> ReadAsync(ReadStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var apiKey = apiKeyProvider();
                if (apiKey == null)
                {
                    log.Warn("Hercules API key is null.");
                    return new ReadStreamResult(HerculesStatus.Unauthorized, null);
                }

                var url = new RequestUrlBuilder("stream/read")
                    .AppendToQuery(Constants.QueryParameters.Stream, query.Name)
                    .AppendToQuery("take", query.Limit)
                    .AppendToQuery("shardIndex", query.ClientShard)
                    .AppendToQuery("shardCount", query.ClientShardCount)
                    .Build();

                var body = CreateRequestBody(query);

                var request = Request
                    .Post(url)
                    .WithHeader(HeaderNames.ContentType, Constants.ContentTypes.OctetStream)
                    .WithHeader(Constants.HeaderNames.ApiKey, apiKey)
                    .WithContent(body);

                var result = await client
                    .SendAsync(request, timeout, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var operationStatus = responseAnalyzer.Analyze(result.Response, out var errorMessage);
                if (operationStatus != HerculesStatus.Success)
                    return new ReadStreamResult(operationStatus, null, errorMessage);

                return new ReadStreamResult(HerculesStatus.Success, ReadResponseBody(result.Response));
            }
            catch (Exception e)
            {
                log.Warn(e);
                return new ReadStreamResult(HerculesStatus.UnknownError, null);
            }
        }

        private static ReadStreamPayload ReadResponseBody(Response response)
        {
            var reader = new BinaryBufferReader(response.Content.Buffer, response.Content.Offset)
            {
                Endianness = Endianness.Big
            };

            var coordinates = StreamCoordinatesReader.Read(reader);

            var events = reader.ReadArray(BinaryEventReader.ReadEvent);

            return new ReadStreamPayload(events, coordinates);
        }

        private static ArraySegment<byte> CreateRequestBody(ReadStreamQuery query)
        {
            var body = new BinaryBufferWriter(
                sizeof(int) + query.Coordinates.Positions.Length * (sizeof(int) + sizeof(long)))
            {
                Endianness = Endianness.Big
            };

            body.Write(query.Coordinates.Positions.Length);

            foreach (var position in query.Coordinates.Positions)
            {
                body.Write(position.Partition);
                body.Write(position.Offset);
            }

            return body.FilledSegment;
        }
    }
}