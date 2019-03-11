using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;

namespace Vostok.Hercules.Client.Sink.StreamState
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
        public AsyncManualResetEvent SendSignal { get; set; } = new AsyncManualResetEvent(false);
    }
}