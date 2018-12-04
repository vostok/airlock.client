using System;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hercules.Client
{
    internal class HerculesStreamClientConfig
    {
        public IClusterProvider Cluster;
        public Func<string> ApiKey;
    }
}