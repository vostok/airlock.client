using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Sink.Analyzer;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;
using Vostok.Hercules.Client.Sink.Writing;

namespace Vostok.Hercules.Client.Sink.State
{
    internal interface IStreamState
    {
        [NotNull]
        string Name { get; }

        [NotNull]
        IBufferPool BufferPool { get; }

        [NotNull]
        IMemoryAnalyzer MemoryAnalyzer { get; }

        [NotNull]
        IRecordWriter RecordWriter { get; }

        [NotNull]
        IStatisticsCollector Statistics { get; }

        [NotNull]
        StreamSettings Settings { get; set; }

        [NotNull]
        AsyncManualResetEvent SendSignal { get; }
    }
}