namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IReadOnlyMemoryManager
    {
        long MaximumSize { get; }
        long Capacity { get; }
        long LastReserveTicks { get; }
    }
}