using System;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal static class Delays
    {
        public static TimeSpan ExponentialWithJitter(TimeSpan sendPeriodCap, TimeSpan sendPeriod, int attempt)
        {
            var baseDelayMs = Math.Min(sendPeriodCap.TotalMilliseconds, sendPeriod.TotalMilliseconds * Math.Pow(2, attempt));
            var delay = TimeSpan.FromMilliseconds(baseDelayMs).Divide(2);
            var jitter = delay.Multiply(ThreadSafeRandom.NextDouble());
            return delay + jitter;
        }
    }
}