using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sending
{
    internal class RequestSender : IRequestSender
    {
        private const string ServiceName = "HerculesGateway";

        private readonly Func<string> getGateApiKey;
        private readonly IClusterClient client;

        public RequestSender(ILog log, HerculesSinkSettings sinkSettings)
        {
            getGateApiKey = sinkSettings.ApiKeyProvider;

            client = new ClusterClient(
                log,
                configuration =>
                {
                    configuration.TargetServiceName = ServiceName;
                    configuration.ClusterProvider = sinkSettings.Cluster;
                    configuration.Transport = new UniversalTransport(log);
                    configuration.DefaultTimeout = 30.Seconds();
                    configuration.DefaultRequestStrategy = Strategy.Forking2;

                    configuration.SetupWeighedReplicaOrdering(builder => builder.AddAdaptiveHealthModifierWithLinearDecay(10.Minutes()));
                    configuration.SetupReplicaBudgeting(configuration.TargetServiceName);
                    configuration.SetupAdaptiveThrottling(configuration.TargetServiceName);

                    sinkSettings.ClusterClientSetup?.Invoke(configuration);
                });
        }

        public async Task<RequestSendingResult> SendAsync(
            string stream,
            CompositeContent content,
            TimeSpan timeout,
            Func<string> apiKeyProvider = null,
            CancellationToken cancellationToken = default)
        {
            var request = Request.Post("stream/sendAsync")
                .WithAdditionalQueryParameter("stream", stream)
                .WithContentTypeHeader("application/octet-stream")
                .WithHeader("apiKey", apiKeyProvider?.Invoke() ?? getGateApiKey())
                .WithContent(content);

            var clusterResult = await client
                .SendAsync(request, cancellationToken: cancellationToken, timeout: timeout)
                .ConfigureAwait(false);

            return GetSendingResult(clusterResult);
        }

        private static RequestSendingResult GetSendingResult(ClusterResult clusterResult) =>
            new RequestSendingResult
            {
                Status = clusterResult.Status,
                Code = clusterResult.Response.Code
            };
    }
}