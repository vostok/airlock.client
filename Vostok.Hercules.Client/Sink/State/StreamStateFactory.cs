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
        private readonly ILog log;

        public StreamStateFactory([NotNull] HerculesSinkSettings settings, [NotNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.log = log;

            globalMemoryManager = new MemoryManager(settings.MaximumMemoryConsumption);
        }

        public IStreamState Create(string name)
        {
            var statistics = new StatisticsCollector();
            var sendSignal = new AsyncManualResetEvent(false);
            var bufferPool = CreateBufferPool();
            var recordWriter = CreateRecordWriter(name, statistics, sendSignal);

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

        private IRecordWriter CreateRecordWriter(string name, IStatisticsCollector statistics, AsyncManualResetEvent signal)
        {
            IRecordWriter writer = new RecordWriter(
                log.ForContext(name),
                () => PreciseDateTime.UtcNow,
                Constants.EventProtocolVersion,
                settings.MaximumRecordSize);

            writer = new ReportingWriter(writer, statistics);

            writer = new SignalingWriter(
                writer,
                statistics,
                signal,
                settings.MaximumPerStreamMemoryConsumption,
                SignalTransitionThreshold,
                SignalConstantThreshold);

            return writer;
        }
    }
}