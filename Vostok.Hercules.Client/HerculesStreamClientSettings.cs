using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents configuration of <see cref="HerculesStreamClient"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesStreamClientSettings
    {
        public HerculesStreamClientSettings([NotNull] IClusterProvider cluster, [NotNull] Func<string> apiKeyProvider)
        {
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            ApiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        }

        /// <summary>
        /// <para>An <see cref="IClusterProvider"/> implementation that provides replicas of Hercules stream API service.</para>
        /// </summary>
        [NotNull]
        public IClusterProvider Cluster { get; }

        /// <summary>
        /// <para>Delegate that returns Hercules stream API key with read access.</para>
        /// </summary>
        [NotNull]
        public Func<string> ApiKeyProvider { get; }

        /// <summary>
        /// <para>An optional delegate that can be used to tune underlying <see cref="IClusterClient"/> instance.</para>
        /// </summary>
        [CanBeNull]
        public ClusterClientSetup AdditionalSetup { get; set; }

        /// <summary>
        /// Maximum size of pooled buffer used for requests.
        /// </summary>
        public int MaxPooledBufferSize { get; set; } = 16 * 1024 * 1024;

        /// <summary>
        /// Maximum amount of pooled buffers per bucket used for requests.
        /// </summary>
        public int MaxPooledBuffersPerBucket { get; set; } = 8;
    }
}