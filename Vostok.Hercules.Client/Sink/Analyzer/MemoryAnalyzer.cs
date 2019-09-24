using System;
using System.Linq;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Analyzer
{
    internal class MemoryAnalyzer : IMemoryAnalyzer
    {
        private readonly IMemoryManager globalMemoryManager;
        private readonly HerculesSinkGcSettings settings;
        private long lastFreeMemoryAttemptTicks;

        public MemoryAnalyzer(IMemoryManager globalMemoryManager, HerculesSinkGcSettings settings)
        {
            this.globalMemoryManager = globalMemoryManager;
            this.settings = settings;
        }

        public bool ShouldFreeMemory(IBufferPool bufferPool)
        {
            var now = DateTime.UtcNow.Ticks;

            if (now - bufferPool.MemoryManager.LastReserveTicks < settings.Period.Ticks)
                return false;

            if (now - lastFreeMemoryAttemptTicks < settings.Cooldown.Ticks)
                return false;

            if (globalMemoryManager.Capacity < settings.MinimumGlobalMemoryLimitUtilization * globalMemoryManager.CapacityLimit)
                return false;

            if (bufferPool.MemoryManager.Capacity < settings.MinimumStreamMemoryLimitUtilization * bufferPool.MemoryManager.CapacityLimit)
                return false;

            if (bufferPool.Count() < settings.MinimumBuffersLimitUtilization)
                return false;

            lastFreeMemoryAttemptTicks = now;
            return true;
        }
    }
}