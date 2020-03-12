using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kontur.Lz4;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Binary;
using Vostok.Commons.Collections;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Serialization.Readers;
using Vostok.Hercules.Client.Serialization.Writers;
using Vostok.Logging.Abstractions;
using BinaryBufferReader = Vostok.Commons.Binary.BinaryBufferReader;

namespace Vostok.Hercules.Client.Internal
{
    internal class StreamApiRequestSender
    {
        private readonly ILog log;
        private readonly BufferPool bufferPool;
        private readonly ClusterClient client;
        private readonly ResponseAnalyzer responseAnalyzer;

        public StreamApiRequestSender(
            [NotNull] IClusterProvider clusterProvider,
            [NotNull] ILog log,
            [NotNull] BufferPool bufferPool,
            [CanBeNull] ClusterClientSetup additionalSetup)
        {
            this.log = log;
            this.bufferPool = bufferPool;

            client = ClusterClientFactory.Create(
                clusterProvider,
                log,
                Constants.ServiceNames.StreamApi,
                config =>
                {
                    config.SetupUniversalTransport(
                        new UniversalTransportSettings
                        {
                            BufferFactory = bufferPool.Rent
                        });
                    config.AddResponseTransform(TryDecompress);
                    additionalSetup?.Invoke(config);
                });

            responseAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Stream);
        }

        public async Task<RawReadStreamResult> ReadAsync(ReadStreamQuery query, string apiKey, TimeSpan timeout, CancellationToken cancellationToken = default)
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
                    .WithAcceptEncodingHeader(Constants.Compression.Lz4Encoding)
                    .WithContent(body);

                if (!string.IsNullOrEmpty(apiKey))
                    request = request.WithHeader(Constants.HeaderNames.ApiKey, apiKey);

                var result = await client
                    .SendAsync(request, timeout, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var operationStatus = responseAnalyzer.Analyze(result.Response, out var errorMessage);
                if (operationStatus != HerculesStatus.Success)
                {
                    if (result.Response.HasContent)
                        bufferPool.Return(result.Response.Content.Buffer);

                    return new RawReadStreamResult(operationStatus, null, errorMessage);
                }

                return new RawReadStreamResult(operationStatus, ParseReadResponseBody(result.Response));
            }
            catch (Exception error)
            {
                log.Warn(error);
                return new RawReadStreamResult(HerculesStatus.UnknownError, null, error.Message);
            }
        }

        public async Task<SeekToEndStreamResult> SeekToEndAsync(SeekToEndStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var url = new RequestUrlBuilder("stream/seekToEnd")
                    {
                        {Constants.QueryParameters.Stream, query.Name},
                        {Constants.QueryParameters.ClientShard, query.ClientShard},
                        {Constants.QueryParameters.ClientShardCount, query.ClientShardCount}
                    }
                    .Build();

                var request = Request
                    .Get(url);

                var result = await client
                    .SendAsync(request, timeout, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    var operationStatus = responseAnalyzer.Analyze(result.Response, out var errorMessage);
                    if (operationStatus != HerculesStatus.Success)
                        return new SeekToEndStreamResult(operationStatus, null, errorMessage);

                    return new SeekToEndStreamResult(operationStatus, ParseSeekToEndResponseBody(result.Response), errorMessage);
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
                return new SeekToEndStreamResult(HerculesStatus.UnknownError, null, error.Message);
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

        private static SeekToEndStreamPayload ParseSeekToEndResponseBody([NotNull] Response response)
        {
            var reader = new BinaryBufferReader(response.Content.Buffer, response.Content.Offset)
            {
                Endianness = Endianness.Big
            };

            var coordinates = StreamCoordinatesReader.Read(reader);

            return new SeekToEndStreamPayload(coordinates);
        }

        private RawReadStreamPayload ParseReadResponseBody([NotNull] Response response)
        {
            try
            {
                var reader = new BinaryBufferReader(response.Content.Buffer, response.Content.Offset)
                {
                    Endianness = Endianness.Big
                };

                var coordinates = StreamCoordinatesReader.Read(reader);

                var content = new ValueDisposable<ArraySegment<byte>>(
                    new ArraySegment<byte>(response.Content.Buffer, (int)reader.Position, (int)(response.Content.Length - reader.Position)),
                    new ActionDisposable(() => bufferPool.Return(response.Content.Buffer)));

                return new RawReadStreamPayload(content, coordinates);
            }
            catch (Exception)
            {
                bufferPool.Return(response.Content.Buffer);
                throw;
            }
        }

        private Response TryDecompress(Response response)
        {
            if (!response.HasContent
                || response.Headers[HeaderNames.ContentEncoding] != Constants.Compression.Lz4Encoding
                || !int.TryParse(response.Headers[Constants.Compression.OriginalContentLengthHeaderName], out var originalContentLength))
                return response;

            return response
                .WithContent(Decompress(response.Content, originalContentLength));
        }

        private Content Decompress(Content content, int originalContentLength)
        {
            var buffer = bufferPool.Rent(originalContentLength);

            var decodedContentLenght = LZ4Codec.Decode(content.Buffer, content.Offset, content.Length, buffer, 0, originalContentLength, true);
            if (decodedContentLenght != originalContentLength)
            {
                bufferPool.Return(buffer);
                throw new Exception($"Failed to decompress {content.Length} bytes: expected exactly {originalContentLength} bytes, but received {decodedContentLenght} bytes.");
            }

            bufferPool.Return(content.Buffer);
            return new Content(buffer, 0, originalContentLength);
        }
    }
}