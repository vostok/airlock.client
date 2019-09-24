using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents configuration of <see cref="HerculesTimelineClient"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesTimelineClientSettings<T>
    {
        public HerculesTimelineClientSettings([NotNull] IClusterProvider cluster, [NotNull] Func<string> apiKeyProvider, [NotNull] Func<IBinaryBufferReader, IHerculesEventBuilder<T>> eventBuilderProvider)
        {
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            ApiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
            EventBuilderProvider = eventBuilderProvider ?? throw new ArgumentNullException(nameof(eventBuilderProvider));
        }

        /// <summary>
        /// <para>An <see cref="IClusterProvider"/> implementation that provides replicas of Hercules timeline API service.</para>
        /// </summary>
        [NotNull]
        public IClusterProvider Cluster { get; }

        /// <summary>
        /// <para>Delegate that returns Hercules stream API key with read access.</para>
        /// </summary>
        [NotNull]
        public Func<string> ApiKeyProvider { get; }

        /// <summary>
        /// <para>Custom hercules events builders provider.</para>
        /// </summary>
        [NotNull]
        public Func<IBinaryBufferReader, IHerculesEventBuilder<T>> EventBuilderProvider { get; }

        /// <summary>
        /// <para>An optional delegate that can be used to tune underlying <see cref="IClusterClient"/> instance.</para>
        /// </summary>
        [CanBeNull]
        public ClusterClientSetup AdditionalSetup { get; set; }
    }
}