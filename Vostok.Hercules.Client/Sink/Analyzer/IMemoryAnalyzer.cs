using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Analyzer
{
    internal interface IMemoryAnalyzer
    {
        bool ShouldFreeMemory(IBufferPool bufferPool);
    }
}