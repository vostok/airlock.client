namespace Vostok.Hercules.Client.Sink
{
    internal interface IMemoryManager
    {
        bool TryReserveBytes(long amount);
        bool IsConsumptionAchievedThreshold(int percent);
    }
}