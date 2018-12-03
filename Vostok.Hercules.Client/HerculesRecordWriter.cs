using System;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Binary;
using Vostok.Hercules.Client.TimeBasedUuid;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordWriter : IHerculesRecordWriter
    {
        private readonly ILog log;
        private readonly ITimeGuidGenerator timeGuidGenerator;
        private readonly byte recordVersion;
        private readonly int maxRecordSize;

        public HerculesRecordWriter(ILog log, ITimeGuidGenerator timeGuidGenerator, byte recordVersion, int maxRecordSize)
        {
            this.log = log;
            this.timeGuidGenerator = timeGuidGenerator;
            this.recordVersion = recordVersion;
            this.maxRecordSize = maxRecordSize;
        }

        public bool TryWrite(IBinaryWriter binaryWriter, Action<IHerculesEventBuilder> build, out int recordSize)
        {
            var startingPosition = binaryWriter.Position;

            try
            {
                binaryWriter.IsOverflowed = false;
                binaryWriter.Write(recordVersion);

                using (var builder = new HerculesEventBuilder(binaryWriter, timeGuidGenerator))
                    build.Invoke(builder);

                if (binaryWriter.IsOverflowed)
                {
                    binaryWriter.Position = startingPosition;
                    recordSize = 0;
                    return false;
                }

            }
            catch (Exception exception)
            {
                log.Error(exception);
                binaryWriter.Position = startingPosition;
                recordSize = 0;
                return false;
            }

            recordSize = binaryWriter.Position - startingPosition;

            if (recordSize <= maxRecordSize)
                return true;

            log.Warn($"Discarded record with size {DataSize.FromBytes(recordSize)} larger than maximum allowed size {DataSize.FromBytes(maxRecordSize)}");
            binaryWriter.Position = startingPosition;
            return false;
        }
    }
}