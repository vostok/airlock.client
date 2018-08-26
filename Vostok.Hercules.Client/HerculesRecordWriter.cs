using System;
using Vostok.Commons.Primitives;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Binary;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordWriter : IHerculesRecordWriter
    {
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

                using (var builder = new HerculesRecordBuilder(binaryWriter))
                    build.Invoke(builder);

                var recordSize = binaryWriter.Position - versionPosition;

                if (recordSize <= maxRecordSize)
                    return true;

                log.Warn($"Discarded record with size {DataSize.FromBytes(recordSize)} larger than maximum allowed size {DataSize.FromBytes(maxRecordSize)}");
                binaryWriter.Position = startingPosition;
                return false;

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