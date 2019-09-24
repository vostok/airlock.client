using System;
using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Sink.Analyzer;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;
using Vostok.Hercules.Client.Sink.Writing;

namespace Vostok.Hercules.Client.Sink.State
{
    internal class StreamState : IStreamState
    {
        public StreamState(
            [NotNull] string name,
            [NotNull] IBufferPool bufferPool,
            [NotNull] IMemoryAnalyzer memoryAnalyzer,
            [NotNull] IRecordWriter recordWriter,
            [NotNull] IStatisticsCollector statistics,
            [NotNull] AsyncManualResetEvent sendSignal)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BufferPool = bufferPool ?? throw new ArgumentNullException(nameof(bufferPool));
            Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
            RecordWriter = recordWriter ?? throw new ArgumentNullException(nameof(recordWriter));
            SendSignal = sendSignal ?? throw new ArgumentNullException(nameof(sendSignal));
            MemoryAnalyzer = memoryAnalyzer ?? throw new ArgumentNullException(nameof(memoryAnalyzer));
        }

        public string Name { get; }

        public IBufferPool BufferPool { get; }

        public IMemoryAnalyzer MemoryAnalyzer { get; }

        public IRecordWriter RecordWriter { get; }

        public IStatisticsCollector Statistics { get; }

        public AsyncManualResetEvent SendSignal { get; }

        public StreamSettings Settings { get; set; } = new StreamSettings();
    }
}