using System;
using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;
using Vostok.Hercules.Client.Sink.Writing;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.State
{
    internal class StreamStateFactory : IStreamStateFactory
    {
        private const int InitialPooledBufferSize = 4096;
        private const double SignalTransitionThreshold = 0.25;
        private const double SignalConstantThreshold = 0.70;

        private readonly HerculesSinkSettings settings;
        private readonly MemoryManager globalMemoryManager;
        private readonly RecordWriter globalRecordWriter;

        public StreamStateFactory([NotNull] HerculesSinkSettings settings, [NotNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            globalMemoryManager = new MemoryManager(settings.MaximumMemoryConsumption);
            globalRecordWriter = new RecordWriter(log, () => PreciseDateTime.UtcNow, 
                Constants.EventProtocolVersion, settings.MaximumRecordSize);
        }

        public IStreamState Create(string name)
        {
            var statistics = new StatisticsCollector();
            var sendSignal = new AsyncManualResetEvent(false);
            var bufferPool = CreateBufferPool();
            var recordWriter = CreateRecordWriter(statistics, sendSignal);

            return new StreamState(name, bufferPool, recordWriter, statistics, sendSignal);
        }

        private IBufferPool CreateBufferPool()
        {
            var privateMemoryManager = new MemoryManager(settings.MaximumPerStreamMemoryConsumption, globalMemoryManager);

            return new BufferPool(
                privateMemoryManager,
                InitialPooledBufferSize,
                settings.MaximumRecordSize,
                settings.MaximumBatchSize);
        }

        private IRecordWriter CreateRecordWriter(IStatisticsCollector statistics, AsyncManualResetEvent signal)
        {
            var writer = globalRecordWriter as IRecordWriter;

            writer = new ReportingWriter(writer, statistics);

            writer = new SignalingWriter(writer, statistics, signal, 
                settings.MaximumPerStreamMemoryConsumption, SignalTransitionThreshold, SignalConstantThreshold);

            return writer;
        }
    }
}
