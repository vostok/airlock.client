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
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Client;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Internal
{
    internal class GateRequestSender : IGateRequestSender
    {
        private readonly ILog log;
        private readonly BufferPool bufferPool;
        private readonly IClusterClient client;
        private readonly ResponseAnalyzer responseAnalyzer;
        private readonly bool compressionEnabled;

        public GateRequestSender(
            [NotNull] IClusterProvider clusterProvider,
            [NotNull] ILog log,
            [NotNull] BufferPool bufferPool,
            [CanBeNull] ClusterClientSetup additionalSetup)
        {
            this.log = log;
            this.bufferPool = bufferPool;
            client = ClusterClientFactory.Create(clusterProvider, log, Constants.ServiceNames.Gate, additionalSetup);
            responseAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Stream);
            compressionEnabled = LZ4Helper.Enabled;
        }

        public Task<InsertEventsResult> SendAsync(string stream, string apiKey, ValueDisposable<Content> content, TimeSpan timeout, CancellationToken cancellationToken) =>
            SendAsync("stream/send", stream, apiKey, content, timeout, cancellationToken);

        public Task<InsertEventsResult> FireAndForgetAsync(string stream, string apiKey, ValueDisposable<Content> content, TimeSpan timeout, CancellationToken cancellationToken) =>
            SendAsync("stream/sendAsync", stream, apiKey, content, timeout, cancellationToken);

        private async Task<InsertEventsResult> SendAsync(
            [NotNull] string path,
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] ValueDisposable<Content> content,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = Request.Post(path)
                    .WithAdditionalQueryParameter(Constants.QueryParameters.Stream, stream)
                    .WithContentTypeHeader(Constants.ContentTypes.OctetStream);

                if (!string.IsNullOrEmpty(apiKey))
                    request = request.WithHeader(Constants.HeaderNames.ApiKey, apiKey);

                if (compressionEnabled)
                {
                    request = request
                        .WithContentEncodingHeader(Constants.Compression.Lz4Encoding)
                        .WithHeader(Constants.Compression.OriginalContentLengthHeaderName, content.Value.Length);
                    content = Compress(content);
                }

                request = request.WithContent(content.Value);

                var result = await client
                    .SendAsync(request, cancellationToken: cancellationToken, timeout: timeout)
                    .ConfigureAwait(false);

                var operationStatus = responseAnalyzer.Analyze(result.Response, out var errorMessage);

                return new InsertEventsResult(operationStatus, errorMessage);
            }
            catch (Exception error)
            {
                log.Warn(error);
                return new InsertEventsResult(HerculesStatus.UnknownError, error.Message);
            }
            finally
            {
                content.Dispose();
            }
        }

        private ValueDisposable<Content> Compress(ValueDisposable<Content> disposableContent)
        {
            var content = disposableContent.Value;
            var maximumCompressedLength = LZ4Codec.CompressBound(content.Length);
            var buffer = bufferPool.Rent(maximumCompressedLength);

            var compressedLength = LZ4Codec.Encode(content.Buffer, content.Offset, content.Length, buffer, 0, buffer.Length);

            if (compressedLength == 0 || compressedLength > maximumCompressedLength)
            {
                bufferPool.Return(buffer);
                throw new Exception($"Failed to compress {content.Length} bytes: expected no more than {maximumCompressedLength} bytes, but received {compressedLength} bytes.");
            }

            disposableContent.Dispose();

            return new ValueDisposable<Content>(
                new Content(buffer, 0, compressedLength),
                new ActionDisposable(() => bufferPool.Return(buffer)));
        }
    }
}