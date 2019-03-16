using System;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Serialization.Builders;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Writing
{
    internal class RecordWriter : IRecordWriter
    {
        private readonly ILog log;
        private readonly Func<DateTimeOffset> timeProvider;
        private readonly byte recordVersion;
        private readonly int maxRecordSize;

        public RecordWriter(ILog log, Func<DateTimeOffset> timeProvider, byte recordVersion, int maxRecordSize)
        {
            this.log = log;
            this.timeProvider = timeProvider;
            this.recordVersion = recordVersion;
            this.maxRecordSize = maxRecordSize;
        }

        public RecordWriteResult TryWrite(IBuffer buffer, Action<IHerculesEventBuilder> build, out int recordSize)
        {
            recordSize = 0;

            var startingPosition = buffer.Position;

            RecordWriteResult RollbackWithError(RecordWriteResult error)
            {
                buffer.Position = startingPosition;
                return error;
            }

            try
            {
                buffer.IsOverflowed = false;
                buffer.Write(recordVersion);

                using (var builder = new BinaryEventBuilder(buffer, timeProvider))
                    build(builder);

                if (buffer.IsOverflowed)
                    return RollbackWithError(RecordWriteResult.OutOfMemory);
            }
            catch (Exception error)
            {
                LogBuilderException(error);

                return RollbackWithError(RecordWriteResult.Exception);
            }

            recordSize = (int)(buffer.Position - startingPosition);

            if (recordSize <= maxRecordSize)
            {
                buffer.CommitRecord(recordSize);
                return RecordWriteResult.Success;
            }

            LogRecordWasTooLarge(recordSize);

            return RollbackWithError(RecordWriteResult.RecordTooLarge);
        }

        private void LogBuilderException(Exception error)
            => log.Error(error, "User-provided record builder has thrown an exception.");

        private void LogRecordWasTooLarge(int recordSize)
            => log.Warn("Discarded record with size {RecordSize} larger than maximum allowed size {MaximumRecordSize}.", recordSize, maxRecordSize);
    }
}