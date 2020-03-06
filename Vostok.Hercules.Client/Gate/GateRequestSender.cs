using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kontur.Lz4;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Commons.Collections;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Hercules.Client.Client;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Gate
{
    internal class GateRequestSender : IGateRequestSender
    {
        private readonly ILog log;
        private readonly BufferPool bufferPool;
        private readonly IClusterClient client;

        public GateRequestSender(
            [NotNull] IClusterProvider clusterProvider,
            [NotNull] ILog log,
            [NotNull] BufferPool bufferPool,
            [CanBeNull] ClusterClientSetup additionalSetup)
        {
            this.log = log;
            this.bufferPool = bufferPool;
            client = ClusterClientFactory.Create(clusterProvider, log, Constants.ServiceNames.Gate, additionalSetup);
        }

        public Task<Response> SendAsync(string stream, string apiKey, ValueDisposable<Content> content, TimeSpan timeout, CancellationToken cancellationToken) =>
            SendAsync("stream/send", stream, apiKey, content, timeout, cancellationToken);

        public Task<Response> FireAndForgetAsync(string stream, string apiKey, ValueDisposable<Content> content, TimeSpan timeout, CancellationToken cancellationToken) =>
            SendAsync("stream/sendAsync", stream, apiKey, content, timeout, cancellationToken);

        private async Task<Response> SendAsync(
            [NotNull] string path,
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] ValueDisposable<Content> content,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var request = Request.Post(path)
                .WithAdditionalQueryParameter(Constants.QueryParameters.Stream, stream)
                .WithContentTypeHeader(Constants.ContentTypes.OctetStream)
                .WithContentEncodingHeader(Constants.Compression.Lz4Encoding)
                .WithHeader(Constants.Compression.OriginalContentLengthHeaderName, content.Value.Length);

            if (!string.IsNullOrEmpty(apiKey))
                request = request.WithHeader(Constants.HeaderNames.ApiKey, apiKey);

            Content compressed;
            try
            {
                compressed = Compress(content.Value);
            }
            catch (Exception e)
            {
                log.Error(e, "Failed to compress content.");
                return new Response(ResponseCode.UnknownFailure);
            }

            content.Dispose();

            try
            {
                request = request.WithContent(compressed);

                var result = await client
                    .SendAsync(request, cancellationToken: cancellationToken, timeout: timeout)
                    .ConfigureAwait(false);

                return result.Response;
            }
            finally
            {
                bufferPool.Return(compressed.Buffer);
            }
        }

        private Content Compress(Content content)
        {
            var maximumCompressedLength = LZ4Codec.CompressBound(content.Length);
            var buffer = bufferPool.Rent(maximumCompressedLength);

            var compressedLength = LZ4Codec.Encode(content.Buffer, content.Offset, content.Length, buffer, 0, buffer.Length);

            if (compressedLength == 0 || compressedLength > maximumCompressedLength)
            {
                bufferPool.Return(buffer);
                throw new Exception($"Failed to compress {content.Length} bytes: expected no more than {maximumCompressedLength} bytes, but received {compressedLength} bytes.");
            }

            return new Content(buffer, 0, compressedLength);
        }
    }
}