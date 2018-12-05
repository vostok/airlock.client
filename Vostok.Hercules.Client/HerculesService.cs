using System;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hercules.Client
{
    public class HerculesService
    {
        public string Name { get; set; }
        
        public IClusterProvider Cluster { get; set; }
        
        public Func<string> ApiKey { get; set; }
    }
}