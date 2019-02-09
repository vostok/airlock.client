using System.Threading;

namespace Vostok.Hercules.Client
{
    internal class MemoryManager : IMemoryManager
    {
        private static readonly MemoryManager Infinite = new MemoryManager(long.MaxValue); 
        
        private readonly long maxSize;
        private readonly IMemoryManager underlyingMemoryManager;
        private long currentSize;

        public MemoryManager(long maxSize, IMemoryManager underlyingMemoryManager=null)
        {
            this.maxSize = maxSize;
            this.underlyingMemoryManager = underlyingMemoryManager ?? Infinite;
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
                        if (underlyingMemoryManager.TryReserveBytes(amount))
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