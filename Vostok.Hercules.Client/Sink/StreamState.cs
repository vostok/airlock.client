using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink
{
    internal class StreamState : IStreamState
    {
        public StreamState(string streamName, IBufferPool bufferPool, IStatisticsCollector statistics)
        {
            StreamName = streamName;
            BufferPool = bufferPool;
            Statistics = statistics;
        }

        public string StreamName { get; }
        public IBufferPool BufferPool { get; }
        public IStatisticsCollector Statistics { get; }
        public StreamSettings Settings { get; set; } = new StreamSettings();
    }
}