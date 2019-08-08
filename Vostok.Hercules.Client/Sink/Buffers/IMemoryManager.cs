namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IMemoryManager
    {
        bool TryReserveBytes(long amount);
        void ReleaseBytes(long amount);
        long EstimateReservedBytes();
    }
}