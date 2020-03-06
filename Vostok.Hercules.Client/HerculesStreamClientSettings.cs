using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Serialization.Readers;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents configuration of <see cref="HerculesStreamClient"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesStreamClientSettings : HerculesStreamClientSettings<HerculesEvent>
    {
        public HerculesStreamClientSettings([NotNull] IClusterProvider cluster, [NotNull] Func<string> apiKeyProvider)
            : base(cluster, apiKeyProvider, _ => new HerculesEventBuilderGeneric())
        {
        }
    }
}