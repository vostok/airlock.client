using System;
using System.Diagnostics;
using System.Threading;

namespace Vostok.Hercules.Client.TimeBasedUuid
{
    internal class PreciseTimestampGenerator
    {
        private const long TicksPerMicrosecond = 10;

        private static readonly double StopwatchTickFrequency = (double) TicksPerMicrosecond * 1000 * 1000 / Stopwatch.Frequency;
        private static readonly object SyncLock = new object();

        private readonly long syncPeriodTicks;
        private readonly long maxAllowedDivergenceTicks;
        private long baseTimestampTicks, lastTimestampTicks, stopwatchStartTimestamp;

        public PreciseTimestampGenerator(TimeSpan syncPeriod, TimeSpan maxAllowedDivergence)
        {
            if (!Stopwatch.IsHighResolution)
                throw new Exception("Stopwatch is not based on a high-resolution timer");
            syncPeriodTicks = syncPeriod.Ticks;
            maxAllowedDivergenceTicks = maxAllowedDivergence.Ticks;
            baseTimestampTicks = DateTime.UtcNow.Ticks;
            lastTimestampTicks = baseTimestampTicks;
            stopwatchStartTimestamp = Stopwatch.GetTimestamp();
        }

        public long NowTicks()
        {
            var lastValue = Volatile.Read(ref lastTimestampTicks);
            while (true)
            {
                var nextValue = GenerateNextTimestamp(lastValue);
                var originalValue = Interlocked.CompareExchange(ref lastTimestampTicks, nextValue, lastValue);
                if (originalValue == lastValue)
                    return nextValue;
                lastValue = originalValue;
            }
        }

        private static long GetDateTimeTicks(long stopwatchTicks)
        {
            double dticks = stopwatchTicks;
            dticks *= StopwatchTickFrequency;
            return unchecked((long) dticks);
        }

        private long GenerateNextTimestamp(long localLastTimestampTicks)
        {
            var nowTicks = DateTime.UtcNow.Ticks;

            var localBaseTimestampTicks = Volatile.Read(ref baseTimestampTicks);
            var stopwatchElapsedTicks = GetDateTimeTicks(Stopwatch.GetTimestamp() - stopwatchStartTimestamp);
            if (stopwatchElapsedTicks > syncPeriodTicks)
            {
                lock (SyncLock)
                {
                    baseTimestampTicks = localBaseTimestampTicks = nowTicks;
                    stopwatchStartTimestamp = Stopwatch.GetTimestamp();
                    stopwatchElapsedTicks = 0;
                }
            }

            var resultTicks = Math.Max(localBaseTimestampTicks + stopwatchElapsedTicks, localLastTimestampTicks + TicksPerMicrosecond);

            if (stopwatchElapsedTicks < 0 || Math.Abs(resultTicks - nowTicks) > maxAllowedDivergenceTicks)
                return Math.Max(nowTicks, localLastTimestampTicks + TicksPerMicrosecond);

            return resultTicks;
        }
    }
}