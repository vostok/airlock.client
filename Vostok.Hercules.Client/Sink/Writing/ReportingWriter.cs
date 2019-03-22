using System;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;

namespace Vostok.Hercules.Client.Sink.Writing
{
    internal class ReportingWriter : IRecordWriter
    {
        private readonly IRecordWriter baseWriter;
        private readonly IStatisticsCollector statistics;

        public ReportingWriter(IRecordWriter baseWriter, IStatisticsCollector statistics)
        {
            this.baseWriter = baseWriter;
            this.statistics = statistics;
        }

        public RecordWriteResult TryWrite(IBuffer buffer, Action<IHerculesEventBuilder> build, out int recordSize)
        {
            var result = baseWriter.TryWrite(buffer, build, out recordSize);

            switch (result)
            {
                case RecordWriteResult.Success:
                    statistics.ReportStoredRecord(recordSize);
                    break;

                case RecordWriteResult.RecordTooLarge:
                    statistics.ReportSizeLimitViolation();
                    break;

                case RecordWriteResult.OutOfMemory:
                    statistics.ReportOverflow();
                    break;

                case RecordWriteResult.Exception:
                    statistics.ReportRecordBuildFailure();
                    break;
            }

            return result;
        }
    }
}