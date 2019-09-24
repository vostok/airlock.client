using System;
using System.Threading;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal class MemoryManager : IMemoryManager
    {
        private readonly IMemoryManager underlyingManager;
        private long currentSize;
        private long lastReserveTicks;

        public MemoryManager(long maxSize, IMemoryManager underlyingManager = null)
        {
            MaximumSize = maxSize;
            this.underlyingManager = underlyingManager;
        }

        public long Capacity => Interlocked.Read(ref currentSize);

        public long LastReserveTicks => Interlocked.Read(ref lastReserveTicks);

        public long MaximumSize { get; }

        public bool TryReserveBytes(long amount)
        {
            Interlocked.Exchange(ref lastReserveTicks, DateTime.UtcNow.Ticks);

            while (true)
            {
                var tCurrentSize = Interlocked.Read(ref currentSize);
                var newSize = tCurrentSize + amount;
                if (newSize <= MaximumSize)
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
    }
}