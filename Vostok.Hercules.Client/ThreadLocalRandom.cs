using System;
using System.Threading;

namespace Vostok.Hercules.Client
{
    internal static class ThreadLocalRandom
    {
        private static readonly ThreadLocal<Random> threadLocalRandom = new ThreadLocal<Random>(
            () =>
            {
                lock (GlobalRandom)
                    return new Random(GlobalRandom.Next());
            });

        private static readonly Random GlobalRandom = new Random(Environment.TickCount);
        public static Random Instance => threadLocalRandom.Value;
    }
}