using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;

namespace Vostok.Hercules.Client.Sink.StreamState
{
    internal interface IStreamState
    {
        string StreamName { get; }
        IBufferPool BufferPool { get; }
        IStatisticsCollector Statistics { get; }
        StreamSettings Settings { get; set; }
        AsyncManualResetEvent SendSignal { get; set; }
    }
}