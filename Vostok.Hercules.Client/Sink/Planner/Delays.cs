using System;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;

namespace Vostok.Hercules.Client.Sink.Planner
{
    internal static class Delays
    {
        public static TimeSpan ExponentialWithJitter(TimeSpan sendPeriodCap, TimeSpan sendPeriod, int attempt)
        {
            const double maxJitterFraction = 0.2;

            var baseDelayMs = Math.Min(sendPeriodCap.TotalMilliseconds, sendPeriod.TotalMilliseconds * Math.Pow(2, attempt));

            var delay = TimeSpan.FromMilliseconds(baseDelayMs);

            var jitter = delay.Multiply(Random(-maxJitterFraction, maxJitterFraction));

            return delay + jitter;
        }

        private static double Random(double from, double to)
            => from + (to - from) * ThreadSafeRandom.NextDouble();
    }
}