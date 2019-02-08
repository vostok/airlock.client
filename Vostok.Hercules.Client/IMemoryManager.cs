namespace Vostok.Hercules.Client
{
    internal interface IMemoryManager
    {
        bool TryReserveBytes(long amount);
        bool IsConsumptionAchievedThreshold(int percent);
    }
}