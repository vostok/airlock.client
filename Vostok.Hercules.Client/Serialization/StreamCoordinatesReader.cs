using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Models;

namespace Vostok.Hercules.Client.Serialization
{
    internal static class StreamCoordinatesReader
    {
        public static StreamCoordinates Read(IBinaryReader reader)
        {
            return new StreamCoordinates(reader.ReadArray(ReadStreamPosition));
        }

        private static StreamPosition ReadStreamPosition(IBinaryReader reader)
        {
            var partition = reader.ReadInt32();
            var offset = reader.ReadInt64();

            return new StreamPosition
            {
                Partition = partition,
                Offset = offset
            };
        }
    }
}