using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kontur.Lz4;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Hercules.Client.Client;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Gate
{
    internal class GateRequestSender : IGateRequestSender
    {
        private readonly ILog log;
        private readonly IClusterClient client;

        public GateRequestSender(
            [NotNull] IClusterProvider clusterProvider,
            [NotNull] ILog log,
            [CanBeNull] ClusterClientSetup additionalSetup)
        {
            this.log = log;
            ClusterClientSetup newAdditionalSetup = configuration =>
            {
                configuration.AddRequestTransform(request => Compress(request));
                additionalSetup?.Invoke(configuration);
            };

            client = ClusterClientFactory.Create(clusterProvider, log, Constants.ServiceNames.Gate, newAdditionalSetup);
        }

        public Task<Response> SendAsync(string stream, string apiKey, Content content, TimeSpan timeout, CancellationToken cancellationToken)
            => SendAsync("stream/send", stream, apiKey, r => r.WithContent(content), timeout, cancellationToken);

        public Task<Response> FireAndForgetAsync(string stream, string apiKey, CompositeContent content, TimeSpan timeout, CancellationToken cancellationToken) =>
            SendAsync("stream/sendAsync", stream, apiKey, r =>
            {
                Console.WriteLine("Adding body");
                return r.WithContent(content);
            }, timeout, cancellationToken);

        private async Task<Response> SendAsync(
            [NotNull] string path,
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] Func<Request, Request> addBody,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("hello1");

            var request = Request.Post(path)
                .WithAdditionalQueryParameter(Constants.QueryParameters.Stream, stream)
                .WithContentTypeHeader(Constants.ContentTypes.OctetStream);

            if (!string.IsNullOrEmpty(apiKey))
                request = request.WithHeader(Constants.HeaderNames.ApiKey, apiKey);

            request = addBody(request);
            
            var result = await client
                .SendAsync(request, cancellationToken: cancellationToken, timeout: timeout)
                .ConfigureAwait(false);

            return result.Response;
        }

        private Request Compress(Request request)
        {
            Console.WriteLine("Compress ..");

            if (request.Content == null)
                return request;

            Console.WriteLine($"Before compress: {request.Content.Length}.");

            request = request
                .WithContentEncodingHeader(Constants.Compression.Lz4Encoding)
                .WithHeader(Constants.Compression.OriginalContentLengthHeaderName, request.Content.Length)
                .WithContent(Compress(request.Content));

            Console.WriteLine($"After compress: {request.Content.Length}.");

            return request;
        }

        private Content Compress(Content content)
        {
            var newContent = LZ4Codec.Encode(content.Buffer, content.Offset, content.Length);
            return new Content(newContent);
        }
    }
}