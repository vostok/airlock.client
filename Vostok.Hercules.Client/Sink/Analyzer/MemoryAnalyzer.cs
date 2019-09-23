using System;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Analyzer
{
    internal class MemoryAnalyzer : IMemoryAnalyzer
    {
        private readonly long freePeriodTicks;

        // CR(iloktionov): Isn't it true that in the absence of any activity this thing will eventually throw out all the buffers..?
        // CR(iloktionov): I vote to add one or more limiting mechanics:
        // CR(iloktionov): 1. Cooldown (probably the dumbest one)
        // CR(iloktionov): 2. Min remaining buffers count
        // CR(iloktionov): 3. Min memory limit utilization (>= x% of allowed memory allocated)

        public MemoryAnalyzer(TimeSpan freePeriod)
        {
            freePeriodTicks = freePeriod.Ticks;
        }

        public bool ShouldFreeMemory(IReadOnlyMemoryManager memoryManager) =>
            DateTime.UtcNow.Ticks - memoryManager.LastReserveTicks >= freePeriodTicks;
    }
}