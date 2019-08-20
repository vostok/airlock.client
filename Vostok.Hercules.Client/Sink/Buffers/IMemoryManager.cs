namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IMemoryManager
    {
        bool TryReserveBytes(long amount);
        void ReleaseBytes(long amount);

        // CR(iloktionov): Why aren't these two just properties?
        long EstimateReservedBytes();
        long LastReserveBytesTicks();
    }
}