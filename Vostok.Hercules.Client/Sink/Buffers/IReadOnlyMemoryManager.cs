namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IReadOnlyMemoryManager
    {
        long CapacityLimit { get; }
        long Capacity { get; }
        long LastReserveTicks { get; }
    }
}