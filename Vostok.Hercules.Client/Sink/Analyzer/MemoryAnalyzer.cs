using System;

namespace Vostok.Hercules.Client.Sink.Analyzer
{
    internal class MemoryAnalyzer : IMemoryAnalyzer
    {
        private readonly long freePeriodTicks;

        public MemoryAnalyzer(TimeSpan freePeriod)
        {
            freePeriodTicks = freePeriod.Ticks;
        }

        public bool ShouldFreeMemory(long lastReserveTicks) =>
            DateTime.UtcNow.Ticks - lastReserveTicks >= freePeriodTicks;
    }
}