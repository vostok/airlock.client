using System;
using System.IO;
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
                var recordBodyLengthPosition = binaryWriter.Position;
                binaryWriter.Write(0);

                var recordBodyStartingPosition = binaryWriter.Position;

                var builder = new AirlockRecordBuilder(binaryWriter);
                build.Invoke(builder);

                var currentPosition = binaryWriter.Position;

                var recordSize = currentPosition - recordBodyStartingPosition;

                if (recordSize > maxRecordSize)
                {
                    binaryWriter.Position = startingPosition;

                    return false;
                }

                binaryWriter.Position = recordBodyLengthPosition;
                binaryWriter.Write(recordSize);

                binaryWriter.Position = currentPosition;

                var timestamp = builder.Timestamp != 0 ? builder.Timestamp : DateTimeOffset.UtcNow.ToUnixTimeNanoseconds();
                binaryWriter.Write(timestamp);

                return true;
            }
            catch (InternalBufferOverflowException)
            {
                binaryWriter.Position = startingPosition;

                return false;
            }
        }
    }
}