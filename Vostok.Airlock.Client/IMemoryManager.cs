namespace Vostok.Airlock.Client
{
    internal interface IMemoryManager
    {
        bool TryReserveBytes(int amount);
    }
}