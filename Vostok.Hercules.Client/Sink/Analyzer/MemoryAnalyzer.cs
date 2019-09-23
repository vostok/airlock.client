using System;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Analyzer
{
    internal class MemoryAnalyzer : IMemoryAnalyzer
    {
        private readonly long freePeriodTicks;

        public MemoryAnalyzer(TimeSpan freePeriod)
        {
            freePeriodTicks = freePeriod.Ticks;
        }

        public bool ShouldFreeMemory(IReadOnlyMemoryManager memoryManager) =>
            DateTime.UtcNow.Ticks - memoryManager.LastReserveTicks >= freePeriodTicks;
    }
}