namespace Vostok.Hercules.Client
{
    internal interface IMemoryManager
    {
        bool TryReserveBytes(int amount);
        bool IsConsumptionAchievedThreshold(int percent);
    }
}