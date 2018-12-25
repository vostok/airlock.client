using System;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hercules.Client
{
    public class HerculesGateClientConfig
    {
        public HerculesGateClientConfig(IClusterProvider cluster, Func<string> apiKeyProvider)
        {
            Cluster = cluster;
            ApiKeyProvider = apiKeyProvider;
        }

        public string ServiceName { get; set; } = "HerculesGateway";
        
        public IClusterProvider Cluster { get; set; }
        
        public Func<string> ApiKeyProvider { get; set; }
    }
}