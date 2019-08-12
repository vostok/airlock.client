using System;
using System.Threading;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal class MemoryManager : IMemoryManager
    {
        private readonly long maxSize;
        private readonly IMemoryManager underlyingManager;
        private long currentSize;
        private long lastReserveTicks;

        public MemoryManager(long maxSize, IMemoryManager underlyingManager = null)
        {
            this.maxSize = maxSize;
            this.underlyingManager = underlyingManager;
        }

        public bool TryReserveBytes(long amount)
        {
            Interlocked.Exchange(ref lastReserveTicks, DateTime.UtcNow.Ticks);

            while (true)
            {
                var tCurrentSize = Interlocked.Read(ref currentSize);
                var newSize = tCurrentSize + amount;
                if (newSize <= maxSize)
                {
                    if (Interlocked.CompareExchange(ref currentSize, newSize, tCurrentSize) == tCurrentSize)
                    {
                        if (underlyingManager == null || underlyingManager.TryReserveBytes(amount))
                            return true;

                        Interlocked.Add(ref currentSize, -amount);
                        return false;
                    }
                }
                else
                    return false;
            }
        }

        public void ReleaseBytes(long amount)
        {
            Interlocked.Add(ref currentSize, -amount);
            underlyingManager?.ReleaseBytes(amount);
        }

        public long EstimateReservedBytes() =>
            Interlocked.Read(ref currentSize);

        public long LastReserveBytesTicks() =>
            Interlocked.Read(ref lastReserveTicks);
    }
}