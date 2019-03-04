using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents a settings of <see cref="HerculesStreamClient"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesStreamClientSettings
    {
        /// <param name="cluster">An <see cref="IClusterProvider"/> implementation that provides replicas of Hercules Stream API service.</param>
        /// <param name="apiKeyProvider">Delegate that returns Hercules gateway API key with read access.</param>
        public HerculesStreamClientSettings(IClusterProvider cluster, Func<string> apiKeyProvider)
        {
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            ApiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        }

        /// <summary>
        /// <para>An <see cref="IClusterProvider"/> implementation that provides replicas of Hercules Stream API service.</para>
        /// </summary>
        public IClusterProvider Cluster { get; set; }

        /// <summary>
        /// <para>Delegate that returns Hercules gateway API key with read access.</para>
        /// </summary>
        public Func<string> ApiKeyProvider { get; set; }
    }
}