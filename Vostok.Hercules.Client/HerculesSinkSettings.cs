using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents configuration of <see cref="HerculesSink"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesSinkSettings
    {
        public HerculesSinkSettings([NotNull] IClusterProvider cluster, [NotNull] Func<string> apiKeyProvider)
        {
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            ApiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        }

        /// <summary>
        /// <para>An <see cref="IClusterProvider"/> implementation that provides replicas of Hercules gate service.</para>
        /// </summary>
        [NotNull]
        public IClusterProvider Cluster { get; }

        /// <summary>
        /// <para>A delegate that returns Hercules gate API key with write access.</para>
        /// <para>API key can be overridden for each stream separately with <see cref="IHerculesSink.ConfigureStream"/> method.</para>
        /// </summary>
        [NotNull]
        public Func<string> ApiKeyProvider { get; }

        /// <summary>
        /// <para>An optional delegate that can be used to tune underlying <see cref="IClusterClient"/> instance.</para>
        /// </summary>
        [CanBeNull]
        public ClusterClientSetup AdditionalSetup { get; set; }

        /// <summary>
        /// <para>A total upper limit (in bytes) on the size of all <see cref="HerculesSink"/> internal buffers.</para>
        /// </summary>
        public long MaximumMemoryConsumption { get; set; } = 128 * 1024 * 1024;

        /// <summary>
        /// <para>A total upper limit (in bytes) on the size of <see cref="HerculesSink"/>'s internal buffers devoted to any single stream.</para>
        /// </summary>
        public long MaximumPerStreamMemoryConsumption { get; set; } = 96 * 1024 * 1024;

        /// <summary>
        /// <para>Maximum size (in bytes) of a single record.</para>
        /// </summary>
        public int MaximumRecordSize { get; set; } = 128 * 1024;

        /// <summary>
        /// <para>Maximum size (in bytes) of a single request body sent to Hercules gate service.</para>
        /// <para>Incidentally, this limit also denotes the maximum size for all internal buffers.</para>
        /// </summary>
        public int MaximumBatchSize { get; set; } = 4 * 1024 * 1024;

        /// <summary>
        /// <para>Base delay between attempts of sending records to Hercules gate.</para>
        /// </summary>
        public TimeSpan SendPeriod { get; set; } = 5.Seconds();

        /// <summary>
        /// <para>Maximum delay between attempts of sending records to Hercules gate.</para>
        /// </summary>
        public TimeSpan SendPeriodCap { get; set; } = 5.Minutes();

        /// <summary>
        /// <para>Timeout of requests to Hercules gate.</para>
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = 30.Seconds();

        /// <summary>
        /// <para>Maximum number of streams whose data might being sent to Hercules gate in parallel.</para>
        /// </summary>
        public int MaxParallelStreams { get; set; } = 2;

        /// <summary>
        /// If set to <c>true</c>, suppresses all verbose request/response logging of <see cref="Vostok.Logging.Abstractions.LogLevel.Info"/> and lower levels.
        /// </summary>
        public bool SuppressVerboseLogging { get; set; } = true;
    }
}
