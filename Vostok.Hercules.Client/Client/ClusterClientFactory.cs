using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Client
{
    internal static class ClusterClientFactory
    {
        [NotNull]
        public static Clusterclient.Core.ClusterClient Create(
            [NotNull] IClusterProvider clusterProvider,
            [NotNull] ILog log,
            [NotNull] string serviceName,
            [CanBeNull] ClusterClientSetup additionalSetup)
        {
            return new Clusterclient.Core.ClusterClient(
                log,
                configuration =>
                {
                    configuration.TargetServiceName = serviceName;
                    configuration.ClusterProvider = clusterProvider;
                    configuration.Transport = new UniversalTransport(log);
                    configuration.DefaultTimeout = 30.Seconds();
                    configuration.DefaultRequestStrategy = Strategy.Forking2;

                    configuration.SetupWeighedReplicaOrdering(builder => builder.AddAdaptiveHealthModifierWithLinearDecay(10.Minutes()));
                    configuration.SetupReplicaBudgeting(configuration.TargetServiceName);
                    configuration.SetupAdaptiveThrottling(configuration.TargetServiceName);
                    configuration.SetupThreadPoolLimitsTuning();

                    additionalSetup?.Invoke(configuration);
                });
        }
    }
}