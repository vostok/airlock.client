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

namespace Vostok.Hercules.Client.Sending
{
    internal class RequestSender : IRequestSender
    {
        private const string ServiceName = "HerculesGateway";
        private const string SynchronousPath = "stream/send";
        private const string FireAndForgetPath = "stream/sendAsync";

        private readonly Func<string> getGateApiKey;
        private readonly IClusterClient client;

        public RequestSender(
            IClusterProvider clusterProvider,
            ILog log,
            Func<string> getGateApiKey,
            ClusterClientSetup clusterClientSetup = null)
        {
            this.getGateApiKey = getGateApiKey;

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

                    clusterClientSetup?.Invoke(configuration);
                });
        }

        public Task<RequestSendingResult> SendAsync(string stream, Content content, TimeSpan timeout, Func<string> apiKeyProvider = null, CancellationToken cancellationToken = default) =>
            SendAsync(SynchronousPath, stream, timeout, apiKeyProvider, cancellationToken, content: content);

        public Task<RequestSendingResult> FireAndForgetAsync(string stream, CompositeContent content, TimeSpan timeout, Func<string> apiKeyProvider = null, CancellationToken cancellationToken = default) =>
            SendAsync(FireAndForgetPath, stream, timeout, apiKeyProvider, cancellationToken, content);

        private async Task<RequestSendingResult> SendAsync(
            string path,
            string stream,
            TimeSpan timeout,
            Func<string> apiKeyProvider,
            CancellationToken cancellationToken,
            CompositeContent compositeContent = null,
            Content content = null)
        {
            var request = Request.Post(path)
                .WithAdditionalQueryParameter("stream", stream)
                .WithContentTypeHeader("application/octet-stream")
                .WithHeader("apiKey", apiKeyProvider?.Invoke() ?? getGateApiKey());

            if (compositeContent != null)
                request = request.WithContent(compositeContent);
            else if (content != null)
                request = request.WithContent(content);

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