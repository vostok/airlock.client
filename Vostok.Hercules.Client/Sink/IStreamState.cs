using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink
{
    internal interface IStreamState
    {
        string StreamName { get; }
        IBufferPool BufferPool { get; }
        IStatisticsCollector Statistics { get; }
        StreamSettings Settings { get; set; }
    }
}