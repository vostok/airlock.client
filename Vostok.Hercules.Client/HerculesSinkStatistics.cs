using System.Collections.Generic;

namespace Vostok.Hercules.Client
{
    public class HerculesSinkStatistics
    {
        /// <summary>
        /// Provides diagnostics information about <see cref="HerculesSink"/> overall.
        /// </summary>
        public HerculesSinkCounters Global { get; internal set; }

        /// <summary>
        /// Provides per-stream diagnostics information.
        /// </summary>
        public IReadOnlyDictionary<string, HerculesSinkCounters> Stream { get; internal set; }
    }
}