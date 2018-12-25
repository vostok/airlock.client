using System;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hercules.Client
{
    public class HerculesStreamClientConfig
    {
        public HerculesStreamClientConfig(IClusterProvider cluster, Func<string> apiKeyProvider)
        {
            Cluster = cluster;
            ApiKeyProvider = apiKeyProvider;
        }

        public string ServiceName { get; set; } = "HerculesStreamApi";
        
        public IClusterProvider Cluster { get; set; }
        
        public Func<string> ApiKeyProvider { get; set; }
    }
}