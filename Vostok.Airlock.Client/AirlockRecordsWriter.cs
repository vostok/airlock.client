using System;
using Vostok.Airlock.Client.Abstractions;
using Vostok.Commons;
using Vostok.Commons.Binary;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Extensions;

namespace Vostok.Airlock.Client
{
    internal class AirlockRecordsWriter : IAirlockRecordsWriter
    {
        private readonly ILog log;
        private readonly int maxRecordSize;

        public AirlockRecordsWriter(ILog log, int maxRecordSize)
        {
            this.log = log;
            this.maxRecordSize = maxRecordSize;
        }

        public bool TryWrite(IBinaryWriter binaryWriter, Action<IAirlockRecordBuilder> build)
        {
            var startingPosition = binaryWriter.Position;

            try
            {
                var recordSizePosition = binaryWriter.Position;
                binaryWriter.Write(0);

                var versionPosition = binaryWriter.Position;
                binaryWriter.Write((byte)1);

                var timestampPosition = binaryWriter.Position;
                binaryWriter.Write(0L);

                var tagsCountPosition = binaryWriter.Position;
                binaryWriter.Write((short)0);

                var builder = new AirlockRecordBuilder(binaryWriter);
                build.Invoke(builder);

                var currentPosition = binaryWriter.Position;

                var recordSize = currentPosition - versionPosition;

                if (recordSize > maxRecordSize)
                {
                    log.Warn($"Discarded record with size {DataSize.FromBytes(recordSize)} larger than maximum allowed size {DataSize.FromBytes(maxRecordSize)}");
                    binaryWriter.Position = startingPosition;
                    return false;
                }

                binaryWriter.Position = recordSizePosition;
                binaryWriter.Write(recordSize);

                var timestamp = builder.Timestamp != 0 ? builder.Timestamp : DateTimeOffset.UtcNow.ToUnixTimeNanoseconds();

                binaryWriter.Position = timestampPosition;
                binaryWriter.Write(timestamp);

                binaryWriter.Position = tagsCountPosition;
                binaryWriter.Write(builder.TagsCount);

                binaryWriter.Position = currentPosition;

                return true;
            }
            catch (Exception exception)
            {
                log.Error(exception);
                binaryWriter.Position = startingPosition;
                return false;
            }
        }
    }
}