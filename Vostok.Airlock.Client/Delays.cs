using System;

namespace Vostok.Airlock.Client
{
    internal static class Delays
    {
        [ThreadStatic] private static Random random;

        private static Random Random => random ?? (random = new Random());

        public static IWithExpotentialDelay Expotential(int sendPeriodCapMs, int sendPeriodMs, int attempt)
        {
            return new ExpotentialDelayContainer(Math.Min(sendPeriodCapMs, sendPeriodMs * (int) Math.Pow(2, attempt)));
        }

        public static IWithPreviousDelay BasedOnPrevious(int previousDelayMs)
        {
            return new PreviousDelayContainer(previousDelayMs);
        }

        private class ExpotentialDelayContainer : IWithExpotentialDelay
        {
            public ExpotentialDelayContainer(int delayMs)
            {
                DelayMs = delayMs;
            }

            public int DelayMs { get; }

            public IWithDelay WithFullJitter()
            {
                return new DelayContainer(Random.Next(0, DelayMs));
            }

            public IWithDelay WithEqualJitter()
            {
                return new DelayContainer(DelayMs / 2 + Random.Next(0, DelayMs / 2));
            }
        }

        private class PreviousDelayContainer : IWithPreviousDelay
        {
            public PreviousDelayContainer(int delayMs)
            {
                DelayMs = delayMs;
            }

            public int DelayMs { get; }

            public IWithDelay WithDecorrelatedJitter(int sendPeriodCapMs, int sendPeriodMs)
            {
                return new DelayContainer(Math.Min(sendPeriodCapMs, Random.Next(sendPeriodMs, DelayMs * 3)));
            }
        }

        private class DelayContainer : IWithDelay
        {
            public DelayContainer(int delayMs)
            {
                DelayMs = delayMs;
            }

            public int DelayMs { get; }
        }
    }
}