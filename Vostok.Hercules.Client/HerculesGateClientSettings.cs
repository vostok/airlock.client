using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents a settings of <see cref="HerculesGateClient"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesGateClientSettings
    {
        public HerculesGateClientSettings(IClusterProvider cluster, Func<string> apiKeyProvider)
        {
            Cluster = cluster;
            ApiKeyProvider = apiKeyProvider;
        }

        /// <summary>
        /// <para>An <see cref="IClusterProvider"/> implementation that provides replicas of Hercules Gateway service.</para>
        /// </summary>
        public IClusterProvider Cluster { get; set; }

        /// <summary>
        /// <para>Delegate that returns Hercules gateway API key with write access.</para>
        /// </summary>
        public Func<string> ApiKeyProvider { get; set; }
    }
}