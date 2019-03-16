using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Hercules.Client.Client;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Gate
{
    internal class GateRequestSender : IGateRequestSender
    {
        private readonly IClusterClient client;

        public GateRequestSender(
            [NotNull] IClusterProvider clusterProvider, 
            [NotNull] ILog log, 
            [CanBeNull] ClusterClientSetup additionalSetup)
        {
            client = ClusterClientFactory.Create(clusterProvider, log, Constants.ServiceNames.Gate, additionalSetup);
        }

        public Task<RequestSendingResult> SendAsync(string stream, string apiKey, Content content, TimeSpan timeout, CancellationToken cancellationToken) 
            => SendAsync("stream/send", stream, apiKey, r => r.WithContent(content), timeout, cancellationToken);

        public Task<RequestSendingResult> FireAndForgetAsync(string stream, string apiKey, CompositeContent content, TimeSpan timeout, CancellationToken cancellationToken) =>
            SendAsync("stream/sendAsync", stream, apiKey, r => r.WithContent(content), timeout, cancellationToken);

        private async Task<RequestSendingResult> SendAsync(
            [NotNull] string path,
            [NotNull] string stream,
            [CanBeNull] string apiKey,
            [NotNull] Func<Request, Request> addBody,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var request = Request.Post(path)
                .WithAdditionalQueryParameter(Constants.QueryParameters.Stream, stream)
                .WithContentTypeHeader(Constants.ContentTypes.OctetStream);

            if (!string.IsNullOrEmpty(apiKey))
                request = request.WithHeader(Constants.HeaderNames.ApiKey, apiKey);

            request = addBody(request);

            var result = await client
                .SendAsync(request, cancellationToken: cancellationToken, timeout: timeout)
                .ConfigureAwait(false);

            return GetSendingResult(result);
        }

        private static RequestSendingResult GetSendingResult(ClusterResult clusterResult) =>
            new RequestSendingResult
            {
                Status = clusterResult.Status,
                Code = clusterResult.Response.Code
            };
    }
}