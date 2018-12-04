using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    internal class RequestSender : IRequestSender
    {
        private readonly Func<string> getGateApiKey;
        private readonly IClusterClient client;

        public RequestSender(ILog log, HerculesGate gate, TimeSpan requestTimeout)
        {
            getGateApiKey = gate.ApiKey;

            client = new ClusterClient(
                log,
                configuration =>
                {
                    configuration.ServiceName = gate.Name;
                    configuration.ClusterProvider = gate.Cluster;
                    configuration.Transport = new UniversalTransport(log);
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
                .WithHeader("apiKey", getGateApiKey())
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