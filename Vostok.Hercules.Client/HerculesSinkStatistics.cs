using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Provides diagnostic information about an instance of <see cref="HerculesSink"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesSinkStatistics
    {
        public HerculesSinkStatistics(
            [NotNull] HerculesSinkCounters total,
            [NotNull] IReadOnlyDictionary<string, HerculesSinkCounters> perStream)
        {
            Total = total ?? throw new ArgumentNullException(nameof(total));
            PerStream = perStream ?? throw new ArgumentNullException(nameof(perStream));
        }

        /// <summary>
        /// Counters summed over all streams.
        /// </summary>
        [NotNull]
        public HerculesSinkCounters Total { get; }

        /// <summary>
        /// Per-stream counters.
        /// </summary>
        [NotNull]
        public IReadOnlyDictionary<string, HerculesSinkCounters> PerStream { get; }
    }
}