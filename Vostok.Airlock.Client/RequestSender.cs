using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Weighed;
using Vostok.ClusterClient.Core.Strategies;
using Vostok.ClusterClient.Core.Topology;
using Vostok.ClusterClient.Transport.Webrequest;
using Vostok.Commons.Conversions;
using Vostok.Logging.Abstractions;

namespace Vostok.Airlock.Client
{
    internal class RequestSender : IRequestSender
    {
        private readonly string gateApiKey;
        private readonly IClusterClient client;

        public RequestSender(ILog log, string gateName, Uri gateUri, string gateApiKey, TimeSpan requestTimeout)
        {
            this.gateApiKey = gateApiKey;

            client = new ClusterClient.Core.ClusterClient(
                log,
                configuration =>
                {
                    configuration.ServiceName = gateName;
                    configuration.ClusterProvider = new FixedClusterProvider(gateUri);
                    configuration.Transport = new WebRequestTransport(log);
                    configuration.DefaultTimeout = requestTimeout;
                    configuration.DefaultRequestStrategy = Strategy.Forking2;
                    
                    configuration.SetupWeighedReplicaOrdering(builder => builder.AddAdaptiveHealthModifierWithLinearDecay(10.Minutes()));
                    configuration.SetupReplicaBudgeting(configuration.ServiceName);
                    configuration.SetupAdaptiveThrottling(configuration.ServiceName);
                });
        }

        public async Task<RequestSendingResult> SendAsync(string stream, ArraySegment<byte> message, CancellationToken cancellationToken = default)
        {
            var request = Request.Post("stream/sendAsync")
                .WithAdditionalQueryParameter("stream", stream)
                .WithContentTypeHeader("application/octet-stream")
                .WithHeader("apiKey", gateApiKey)
                .WithContent(message);

            var clusterResult = await client.SendAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            return GetSendingResult(clusterResult);
        }

        private static RequestSendingResult GetSendingResult(ClusterResult clusterResult)
        {
            switch (clusterResult.Status)
            {
                case ClusterResultStatus.Success:
                    return clusterResult.Response.IsSuccessful
                        ? RequestSendingResult.Success
                        : RequestSendingResult.DefinitiveFailure;

                case ClusterResultStatus.TimeExpired:
                case ClusterResultStatus.ReplicasExhausted:
                case ClusterResultStatus.Throttled:
                    return RequestSendingResult.IntermittentFailure;

                case ClusterResultStatus.ReplicasNotFound:
                case ClusterResultStatus.IncorrectArguments:
                case ClusterResultStatus.UnexpectedException:
                case ClusterResultStatus.Canceled:
                    return RequestSendingResult.DefinitiveFailure;
                    
                default:
                    throw new ArgumentOutOfRangeException(nameof(clusterResult.Status));
            }
        }
    }
}