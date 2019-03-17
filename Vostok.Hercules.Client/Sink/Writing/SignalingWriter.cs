using System;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;

namespace Vostok.Hercules.Client.Sink.Writing
{
    internal class SignalingWriter : IRecordWriter
    {
        private readonly IRecordWriter baseWriter;
        private readonly IStatisticsCollector statistics;
        private readonly AsyncManualResetEvent signal;
        private readonly long transitionSignalThreshold;
        private readonly long constantSignalThreshold;

        public SignalingWriter(
            IRecordWriter baseWriter, 
            IStatisticsCollector statistics, 
            AsyncManualResetEvent signal, 
            long sizeLimit,
            double transitionSignalFraction,
            double constantSignalFraction)
        {
            this.baseWriter = baseWriter;
            this.statistics = statistics;
            this.signal = signal;

            transitionSignalThreshold = (long)(sizeLimit * transitionSignalFraction);
            constantSignalThreshold = (long)(sizeLimit * constantSignalFraction);
        }

        public RecordWriteResult TryWrite(IBuffer buffer, Action<IHerculesEventBuilder> build, out int recordSize)
        {
            var storedSizeBefore = statistics.EstimateStoredSize();

            var result = baseWriter.TryWrite(buffer, build, out recordSize);

            var storedSizeAfter = statistics.EstimateStoredSize();

            if (storedSizeAfter >= constantSignalThreshold ||
                storedSizeAfter > 0 && result == RecordWriteResult.OutOfMemory ||
                storedSizeBefore < transitionSignalThreshold && storedSizeAfter >= transitionSignalThreshold)
            {
                signal.Set();
            }

            return result;
        }
    }
}