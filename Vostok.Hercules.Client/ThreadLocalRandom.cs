using System;
using System.Threading;

namespace Vostok.Hercules.Client
{
    internal static class ThreadLocalRandom
    {
        private static readonly ThreadLocal<Random> threadLocalRandom = new ThreadLocal<Random>(
            () =>
            {
                lock (globalRandom)
                    return new Random(globalRandom.Next());
            });

        private static readonly Random globalRandom = new Random(Environment.TickCount);

        public static Random Instance => threadLocalRandom.Value;
    }
}