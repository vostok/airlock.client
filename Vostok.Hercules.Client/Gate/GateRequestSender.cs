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
    internal class GateRequestSender : IGateRequestSender
    {
        private const int MaxPooledBufferSize = 16 * 1024 * 1024;
        private const int MaxPooledBuffersPerBucket = 8;

        private readonly IClusterClient client;
        private readonly BufferPool bufferPool;

        public GateRequestSender(
            [NotNull] IClusterProvider clusterProvider,
            [NotNull] ILog log,
            [CanBeNull] ClusterClientSetup additionalSetup)
        {
            client = ClusterClientFactory.Create(clusterProvider, log, Constants.ServiceNames.Gate, additionalSetup);
            bufferPool = new BufferPool(MaxPooledBufferSize, MaxPooledBuffersPerBucket);
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
            var buffer = bufferPool.Rent(content.Length + 1024);
            var length = LZ4Codec.Encode(content.Buffer, content.Offset, content.Length, buffer, 0, buffer.Length);
            return new Content(buffer, 0, length);
        }
    }
}