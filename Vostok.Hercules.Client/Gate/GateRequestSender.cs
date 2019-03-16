using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Gate
{
    internal class GateRequestSender : IGateRequestSender
    {
        private const string ServiceName = "HerculesGateway";

        private readonly IClusterClient client;

        public GateRequestSender(
            [NotNull] IClusterProvider clusterProvider, 
            [NotNull] ILog log, 
            [CanBeNull] ClusterClientSetup additionalSetup = null)
        {
            client = new ClusterClient(
                log,
                configuration =>
                {
                    configuration.TargetServiceName = ServiceName;
                    configuration.ClusterProvider = clusterProvider;
                    configuration.Transport = new UniversalTransport(log);
                    configuration.DefaultTimeout = 30.Seconds();
                    configuration.DefaultRequestStrategy = Strategy.Forking2;

                    configuration.SetupWeighedReplicaOrdering(builder => builder.AddAdaptiveHealthModifierWithLinearDecay(10.Minutes()));
                    configuration.SetupReplicaBudgeting(configuration.TargetServiceName);
                    configuration.SetupAdaptiveThrottling(configuration.TargetServiceName);

                    additionalSetup?.Invoke(configuration);
                });
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
                .WithAdditionalQueryParameter(Constants.StreamQueryParameter, stream)
                .WithContentTypeHeader(Constants.OctetStreamContentType);

            if (!string.IsNullOrEmpty(apiKey))
                request = request.WithHeader(Constants.ApiKeyHeaderName, apiKey);

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