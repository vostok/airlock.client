﻿using System.Threading;

namespace Vostok.Hercules.Client
{
    internal class MemoryManager : IMemoryManager
    {
        private readonly long maxSize;
        private long currentSize;

        public MemoryManager(long maxSize)
        {
            this.maxSize = maxSize;
        }

        public bool TryReserveBytes(int amount)
        {
            while (true)
            {
                var tCurrentSize = currentSize;
                var newSize = tCurrentSize + amount;
                if (newSize <= maxSize)
                {
                    if (Interlocked.CompareExchange(ref currentSize, newSize, tCurrentSize) == tCurrentSize)
                        return true;
                }
                else
                    return false;
            }
        }

        public bool IsConsumptionAchievedThreshold(int percent) =>
            currentSize * (100.0 / percent) > maxSize;
    }
}