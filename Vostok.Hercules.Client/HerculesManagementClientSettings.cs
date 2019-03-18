using System;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents a settings of <see cref="HerculesManagementClient"/>.
    /// </summary>
    public class HerculesManagementClientSettings
    {
        public HerculesManagementClientSettings(IClusterProvider cluster, Func<string> apiKeyProvider)
        {
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            ApiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        }

        public IClusterProvider Cluster { get; set; }
        public Func<string> ApiKeyProvider { get; set; }
        public ClusterClientSetup ClusterClientSetup { get; set; }
    }
}