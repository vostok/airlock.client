using System;
using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;

namespace Vostok.Hercules.Client.Sink.StreamState
{
    internal class StreamState : IStreamState
    {
        public StreamState([NotNull] string name, [NotNull] IBufferPool bufferPool, [NotNull] IStatisticsCollector statistics)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BufferPool = bufferPool ?? throw new ArgumentNullException(nameof(bufferPool));
            Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        }

        public string Name { get; }

        public IBufferPool BufferPool { get; }

        public IStatisticsCollector Statistics { get; }

        public StreamSettings Settings { get; set; } = new StreamSettings();

        public AsyncManualResetEvent SendSignal { get; } = new AsyncManualResetEvent(false);
    }
}