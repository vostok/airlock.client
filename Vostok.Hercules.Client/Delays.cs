using System;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;

namespace Vostok.Hercules.Client
{
    internal static class Delays
    {
        public static TimeSpan ExponentialWithJitter(TimeSpan sendPeriodCap, TimeSpan sendPeriod, int attempt)
        {
            var delayMs = Math.Min(sendPeriodCap.TotalMilliseconds, sendPeriod.TotalMilliseconds * Math.Pow(2, attempt));
            var delay = TimeSpan.FromMilliseconds(delayMs);
            var jitter = delay.Multiply(ThreadSafeRandom.NextDouble() - 0.5);
            return delay + jitter;
        }
    }
}