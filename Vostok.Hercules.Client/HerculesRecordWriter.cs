using System;
using Vostok.Commons.Primitives;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Binary;
using Vostok.Hercules.Client.TimeBasedUuid;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordWriter : IHerculesRecordWriter
    {
        private static readonly byte[] TimeGuidBytesCap = new byte[TimeGuid.Size];

        private readonly ILog log;
        private readonly int maxRecordSize;

        public HerculesRecordWriter(ILog log, int maxRecordSize)
        {
            this.log = log;
            this.maxRecordSize = maxRecordSize;
        }

        public bool TryWrite(IBinaryWriter binaryWriter, Action<IHerculesRecordBuilder> build)
        {
            var startingPosition = binaryWriter.Position;

            try
            {
                var versionPosition = binaryWriter.Position;
                binaryWriter.Write((byte)1);

                var timeGuidPosition = binaryWriter.Position;
                binaryWriter.WriteWithoutLengthPrefix(TimeGuidBytesCap);

                var tagsCountPosition = binaryWriter.Position;
                binaryWriter.WriteInNetworkByteOrder((short)0);

                var builder = new HerculesRecordBuilder(binaryWriter);
                build.Invoke(builder);

                var currentPosition = binaryWriter.Position;

                var recordSize = currentPosition - versionPosition;

                if (recordSize > maxRecordSize)
                {
                    log.Warn($"Discarded record with size {DataSize.FromBytes(recordSize).ToString()} larger than maximum allowed size {DataSize.FromBytes(maxRecordSize).ToString()}");
                    binaryWriter.Position = startingPosition;
                    return false;
                }

                binaryWriter.Position = timeGuidPosition;
                var timeGuid = builder.Timestamp != 0
                    ? TimeGuid.New(builder.Timestamp)
                    : TimeGuid.Now();
                binaryWriter.WriteWithoutLengthPrefix(timeGuid.ToByteArray());

                binaryWriter.Position = tagsCountPosition;
                binaryWriter.WriteInNetworkByteOrder(builder.TagsCount);

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