using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Serialization.Readers;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents configuration of <see cref="HerculesTimelineClient"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesTimelineClientSettings : HerculesTimelineClientSettings<HerculesEvent>
    {
        public HerculesTimelineClientSettings([NotNull] IClusterProvider cluster, [NotNull] Func<string> apiKeyProvider)
            : base(cluster, apiKeyProvider, _ => new HerculesEventBuilderGeneric())
        {
        }
    }
}