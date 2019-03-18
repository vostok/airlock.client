using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents configuration of <see cref="HerculesManagementClient"/>.
    /// </summary>
    public class HerculesManagementClientSettings
    {
        public HerculesManagementClientSettings([NotNull] IClusterProvider cluster, [NotNull] Func<string> apiKeyProvider)
        {
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            ApiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        }

        /// <summary>
        /// <para>An <see cref="IClusterProvider"/> implementation that provides replicas of Hercules management API service.</para>
        /// </summary>
        [NotNull]
        public IClusterProvider Cluster { get; }

        /// <summary>
        /// <para>Delegate that returns Hercules gate API key (required access level varies per operation).</para>
        /// </summary>
        [NotNull]
        public Func<string> ApiKeyProvider { get; }

        /// <summary>
        /// <para>An optional delegate that can be used to tune underlying <see cref="IClusterClient"/> instance.</para>
        /// </summary>
        [CanBeNull]
        public ClusterClientSetup AdditionalSetup { get; set; }
    }
}