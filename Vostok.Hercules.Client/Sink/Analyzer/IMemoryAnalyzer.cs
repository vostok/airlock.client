namespace Vostok.Hercules.Client.Sink.Analyzer
{
    internal interface IMemoryAnalyzer
    {
        bool ShouldFreeMemory(long lastReserveTicks);
    }
}