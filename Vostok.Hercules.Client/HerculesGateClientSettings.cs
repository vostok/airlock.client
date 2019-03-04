﻿using System;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hercules.Client
{
    public class HerculesGateClientSettings
    {
        public HerculesGateClientSettings(IClusterProvider cluster, Func<string> apiKeyProvider)
        {
            Cluster = cluster;
            ApiKeyProvider = apiKeyProvider;
        }

        public IClusterProvider Cluster { get; set; }

        public Func<string> ApiKeyProvider { get; set; }
    }
}