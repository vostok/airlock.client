using System;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;

namespace Vostok.Hercules.Client.Sink.Planning
{
    internal static class Delays
    {
        public static TimeSpan ExponentialWithJitter(
            TimeSpan sendPeriodCap, 
            TimeSpan sendPeriod, 
            int attempt,
            double maxJitterFraction)
        {
            var baseDelayMs = Math.Min(sendPeriodCap.TotalMilliseconds, sendPeriod.TotalMilliseconds * Math.Pow(2, attempt));

            var delay = TimeSpan.FromMilliseconds(baseDelayMs);

            var jitter = delay.Multiply(Random(-maxJitterFraction, maxJitterFraction));

            return delay + jitter;
        }

        private static double Random(double from, double to)
            => from + (to - from) * ThreadSafeRandom.NextDouble();
    }
}