using System;
using System.Threading;

namespace Vostok.Airlock.Client
{
    internal static class ThreadLocalRandom
    {
        public static Random Instance => threadLocalRandom.Value;

        private static readonly ThreadLocal<Random> threadLocalRandom = new ThreadLocal<Random>(() =>
        {
            lock (globalRandom)
                return new Random(globalRandom.Next());
        });

        private static readonly Random globalRandom = new Random(Environment.TickCount);
    }
}