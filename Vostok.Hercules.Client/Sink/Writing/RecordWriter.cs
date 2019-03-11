using System;
using Vostok.Commons.Primitives;
using Vostok.Hercules.Client.Abstractions.Events;
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

        public WriteResult TryWrite(IBuffer binaryWriter, Action<IHerculesEventBuilder> build, out int recordSize)
        {
            var startingPosition = binaryWriter.Position;

            try
            {
                binaryWriter.IsOverflowed = false;
                binaryWriter.Write(recordVersion);

                using (var builder = new EventBuilder(binaryWriter, timeProvider))
                    build.Invoke(builder);

                if (binaryWriter.IsOverflowed)
                {
                    binaryWriter.Position = startingPosition;
                    recordSize = 0;
                    return WriteResult.OutOfMemory;
                }
            }
            catch (Exception exception)
            {
                binaryWriter.Position = startingPosition;
                recordSize = 0;
                log.Error(exception);
                return WriteResult.Exception;
            }

            recordSize = (int)(binaryWriter.Position - startingPosition);

            if (recordSize <= maxRecordSize)
                return WriteResult.NoError;

            log.Warn("Discarded record with size {RecordSize} larger than maximum allowed size {MaximumRecordSize}", DataSize.FromBytes(recordSize), DataSize.FromBytes(maxRecordSize));
            binaryWriter.Position = startingPosition;
            return WriteResult.RecordTooLarge;
        }
    }
}