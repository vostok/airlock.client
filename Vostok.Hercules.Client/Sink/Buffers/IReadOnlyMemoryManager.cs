namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IReadOnlyMemoryManager
    {
        long Capacity { get; }
        long LastReserveTicks { get; }
    }
}