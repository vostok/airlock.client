namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IMemoryManager : IReadOnlyMemoryManager
    {
        bool TryReserveBytes(long amount);
        void ReleaseBytes(long amount);
    }
}