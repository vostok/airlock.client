using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;

namespace Vostok.Hercules.Client.Sink.StreamState
{
    internal interface IStreamState
    {
        [NotNull]
        string Name { get; }

        [NotNull]
        IBufferPool BufferPool { get; }

        [NotNull]
        IStatisticsCollector Statistics { get; }

        [NotNull]
        StreamSettings Settings { get; set; }

        [NotNull]
        AsyncManualResetEvent SendSignal { get; set; }
    }
}