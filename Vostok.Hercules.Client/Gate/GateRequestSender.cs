using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kontur.Lz4;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Commons.Collections;
using Vostok.Hercules.Client.Client;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Gate
{
    // CR(iloktionov): Dispose of the outer pooled buffer ASAP (be sure to protect against double dispose though)

    internal class GateRequestSender : IGateRequestSender
    {
        private readonly BufferPool bufferPool;
        private readonly IClusterClient client;

        public GateRequestSender(
            [NotNull] IClusterProvider clusterProvider,
            [NotNull] ILog log,
            [NotNull] BufferPool bufferPool,
            [CanBeNull] ClusterClientSetup additionalSetup)
        {
            this.bufferPool = bufferPool;
            client = ClusterClientFactory.Create(clusterProvider, log, Constants.ServiceNames.Gate, additionalSetup);
        }

        public Task<Response> SendAsync(string stream, string apiKey, Content content, TimeSpan timeout, CancellationToken cancellationToken) =>
            SendAsync("stream/send", stream, apiKey, content, timeout, cancellationToken);

        public Task<Response> FireAndForgetAsync(string stream, string apiKey, Content content, TimeSpan timeout, CancellationToken cancellationToken) =>
            SendAsync("stream/sendAsync", stream, apiKey, content, timeout, cancellationToken);

        private async Task<Response> SendAsync(
            [NotNull] string path,
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] Content content,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var request = Request.Post(path)
                .WithAdditionalQueryParameter(Constants.QueryParameters.Stream, stream)
                .WithContentTypeHeader(Constants.ContentTypes.OctetStream)
                .WithContentEncodingHeader(Constants.Compression.Lz4Encoding)
                .WithHeader(Constants.Compression.OriginalContentLengthHeaderName, content.Length);

            if (!string.IsNullOrEmpty(apiKey))
                request = request.WithHeader(Constants.HeaderNames.ApiKey, apiKey);

            content = Compress(content);

            try
            {
                request = request.WithContent(content);

                var result = await client
                    .SendAsync(request, cancellationToken: cancellationToken, timeout: timeout)
                    .ConfigureAwait(false);

                return result.Response;
            }
            finally
            {
                bufferPool.Return(content.Buffer);
            }
        }

        private Content Compress(Content content)
        {
            // CR(iloktionov): Some proof of this formula's correctness would be nice. How do we know for sure that compressed content can't grow larger?
            var maximumCompressedLength = content.Length + content.Length / 255 + 1024;
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