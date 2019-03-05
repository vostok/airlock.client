namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IMemoryManager
    {
        bool TryReserveBytes(long amount);
        bool IsConsumptionAchievedThreshold(int percent);
    }
}