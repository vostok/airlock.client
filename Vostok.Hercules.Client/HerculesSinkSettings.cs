using System;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Commons.Primitives;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents a settings of <see cref="HerculesSink"/>.
    /// </summary>
    public class HerculesSinkSettings
    {
        /// <param name="cluster">>An <see cref="IClusterProvider"/> implementation that provides replicas of Hercules gateway service.</param>
        /// <param name="apiKeyProvider">Delegate that returns Hercules gateway API key with write access.</param>
        public HerculesSinkSettings(IClusterProvider cluster, Func<string> apiKeyProvider)
        {
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            ApiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        }

        /// <summary>
        /// <para>An <see cref="IClusterProvider"/> implementation that provides replicas of Hercules gateway service.</para>
        /// </summary>
        public IClusterProvider Cluster { get; set; }

        /// <summary>
        /// <para>An optional delegate that can be used to tune underlying <see cref="IClusterClient"/> instance.</para>
        /// </summary>
        public ClusterClientSetup ClusterClientSetup { get; set; }

        /// <summary>
        /// <para>Delegate that returns Hercules gateway API key with write access.</para>
        /// <para>API key can be overridden for each stream separately with <see cref="IHerculesSink.ConfigureStream"/> method.</para>
        /// </summary>
        public Func<string> ApiKeyProvider { get; set; }

        /// <summary>
        /// <para>How much memory (in bytes) can take a buffers with records.</para>
        /// </summary>
        public long MaximumMemoryConsumption { get; set; } = 128 * DataSizeConstants.Megabyte;

        /// <summary>
        /// <para>How much memory (in bytes) can take a buffers with records for same stream.</para>
        /// </summary>
        public long MaximumPerStreamMemoryConsumption { get; set; } = 64 * DataSizeConstants.Megabyte;

        /// <summary>
        /// <para>Maximum size (in bytes) of single record.</para>
        /// </summary>
        public int MaximumRecordSize { get; set; } = 128 * (int)DataSizeConstants.Kilobyte;

        /// <summary>
        /// <para>Maximum size (in bytes) of single buffer with records which will be stored in memory and transferred by network to Hercules gateway.</para>
        /// </summary>
        public int MaximumBatchSize { get; set; } = 4 * (int)DataSizeConstants.Megabyte;

        /// <summary>
        /// <para>Base delay between attempts of sending records to Hercules gateway.</para>
        /// </summary>
        public TimeSpan RequestSendPeriod { get; set; } = 2.Seconds();

        /// <summary>
        /// <para>Maximum delay between attempts of sending records to Hercules gateway.</para>
        /// </summary>
        public TimeSpan RequestSendPeriodCap { get; set; } = 5.Minutes();

        /// <summary>
        /// <para>Timeout of requests to Hercules gateway.</para>
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = 30.Seconds();
    }
}