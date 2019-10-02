using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// Represents configuration of garbage collection of inner <see cref="HerculesSink"/>'s buffers.
    /// </summary>
    [PublicAPI]
    public class HerculesSinkGcSettings
    {
        // CR(iloktionov): Rename according to its meaning.
        /// <summary>
        /// Base delay between attempts of removing extra buffers.
        /// </summary>
        public TimeSpan Period { get; set; } = 10.Minutes();

        /// <summary>
        /// Base cooldown between attempts of removing extra buffers.
        /// </summary>
        public TimeSpan Cooldown { get; set; } = 1.Minutes();

        /// <summary>
        /// Minimum amount of total reserved memory by all streams, needed for triggering removing extra buffers.
        /// </summary>
        public double MinimumGlobalMemoryLimitUtilization { get; set; } = 0.2;

        /// <summary>
        /// Minimum amount of reserved memory by current stream, needed for triggering removing extra buffers.
        /// </summary>
        public double MinimumStreamMemoryLimitUtilization { get; set; } = 0.2;

        // CR(iloktionov): Transform this into just the minimum nonreducible number of buffers (1 by default?). Rename accordingly.
        /// <summary>
        /// Minimum amount of allocated buffers, needed for triggering removing extra buffers.
        /// </summary>
        public int MinimumBuffersLimitUtilization { get; set; } = 2;
    }
}