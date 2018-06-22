﻿using System;

namespace Vostok.Airlock.Client
{
    internal static class Delays
    {
        [ThreadStatic] private static Random random;

        private static Random Random => random ?? (random = new Random());

        public static IWithExponentialDelay Exponential(TimeSpan sendPeriodCap, TimeSpan sendPeriod, int attempt)
        {
            var delayMs = Math.Min(sendPeriodCap.TotalMilliseconds, sendPeriod.TotalMilliseconds * Math.Pow(2, attempt));
            return new ExponentialDelayContainer(TimeSpan.FromMilliseconds(delayMs));
        }

        public static IWithPreviousDelay BasedOnPrevious(TimeSpan previousDelay)
        {
            return new PreviousDelayContainer(previousDelay);
        }

        private class ExponentialDelayContainer : IWithExponentialDelay
        {
            public ExponentialDelayContainer(TimeSpan delay)
            {
                Value = delay;
            }

            public TimeSpan Value { get; }

            public IWithDelay WithFullJitter()
            {
                var delayMs = Random.NextDouble() * Value.TotalMilliseconds;
                return new DelayContainer(TimeSpan.FromMilliseconds(delayMs));
            }

            public IWithDelay WithEqualJitter()
            {
                var delayMs = Value.TotalMilliseconds / 2 + Random.NextDouble() * (Value.TotalMilliseconds / 2);
                return new DelayContainer(TimeSpan.FromMilliseconds(delayMs));
            }
        }

        private class PreviousDelayContainer : IWithPreviousDelay
        {
            public PreviousDelayContainer(TimeSpan delay)
            {
                Value = delay;
            }

            public TimeSpan Value { get; }

            public IWithDelay WithDecorrelatedJitter(TimeSpan sendPeriodCap, TimeSpan sendPeriod)
            {
                var delayMs = Math.Min(sendPeriodCap.TotalMilliseconds, Math.Min(Value.TotalMilliseconds * 3, sendPeriod.TotalMilliseconds + Random.NextDouble() * Value.TotalMilliseconds * 3));
                return new DelayContainer(TimeSpan.FromMilliseconds(delayMs));
            }
        }

        private class DelayContainer : IWithDelay
        {
            public DelayContainer(TimeSpan delay)
            {
                Value = delay;
            }

            public TimeSpan Value { get; }
        }
    }
}