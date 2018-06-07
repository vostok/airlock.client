using System;
using Vostok.Airlock.Client.Abstractions;
using Vostok.Commons.Binary;

namespace Vostok.Airlock.Client
{
    internal class AirlockRecordWriter : IAirlockRecordWriter
    {
        private readonly int maxRecordSize;

        public AirlockRecordWriter(int maxRecordSize)
        {
            this.maxRecordSize = maxRecordSize;
        }

        public bool TryWrite(IBinaryWriter binaryWriter, Action<IAirlockRecordBuilder> build)
        {
            var startingPosition = binaryWriter.Position;

            try
            {
                var timestampPosition = binaryWriter.Position;
                binaryWriter.Write(0L);

                var recordBodySizePosition = binaryWriter.Position;
                binaryWriter.Write(0);

                var recordBodyStartingPosition = binaryWriter.Position;

                var builder = new AirlockRecordBuilder(binaryWriter);
                build.Invoke(builder);

                var currentPosition = binaryWriter.Position;

                var recordSize = currentPosition - startingPosition + 1;

                if (recordSize > maxRecordSize)
                {
                    binaryWriter.Position = startingPosition;

                    return false;
                }

                var timestamp = builder.Timestamp != 0 ? builder.Timestamp : DateTimeOffset.UtcNow.ToUnixTimeNanoseconds();

                binaryWriter.Position = timestampPosition;
                binaryWriter.Write(timestamp);

                var recordBodySize = currentPosition - recordBodyStartingPosition;

                binaryWriter.Position = recordBodySizePosition;
                binaryWriter.Write(recordBodySize);

                binaryWriter.Position = currentPosition;

                return true;
            }
            catch
            {
                binaryWriter.Position = startingPosition;

                return false;
            }
        }
    }
}