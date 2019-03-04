using System.Threading;

namespace Vostok.Hercules.Client.Sink
{
    internal class MemoryManager : IMemoryManager
    {
        private readonly long maxSize;
        private readonly IMemoryManager underlyingMemoryManager;
        private long currentSize;

        public MemoryManager(long maxSize, IMemoryManager underlyingMemoryManager = null)
        {
            this.maxSize = maxSize;
            this.underlyingMemoryManager = underlyingMemoryManager;
        }

        public bool TryReserveBytes(long amount)
        {
            while (true)
            {
                var tCurrentSize = Interlocked.Read(ref currentSize);
                var newSize = tCurrentSize + amount;
                if (newSize <= maxSize)
                {
                    if (Interlocked.CompareExchange(ref currentSize, newSize, tCurrentSize) == tCurrentSize)
                    {
                        if (underlyingMemoryManager?.TryReserveBytes(amount) ?? true)
                            return true;
                        Interlocked.Add(ref currentSize, -amount);
                        return false;
                    }
                }
                else
                    return false;
            }
        }

        public bool IsConsumptionAchievedThreshold(int percent) =>
            currentSize * (100.0 / percent) > maxSize;
    }
}