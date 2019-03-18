using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Serialization.Helpers;

namespace Vostok.Hercules.Client.Serialization.Readers
{
    internal static class TimelineCoordinatesReader
    {
        private const int EventIdSize = 24;

        public static TimelineCoordinates Read(IBinaryReader reader)
        {
            return new TimelineCoordinates(reader.EnsureBigEndian().ReadArray(ReadTimelinePosition));
        }

        private static TimelinePosition ReadTimelinePosition(IBinaryReader reader)
        {
            var slice = reader.ReadInt32();
            var offset = reader.ReadInt64();
            var eventId = reader.ReadByteArray(EventIdSize);

            return new TimelinePosition
            {
                Slice = slice,
                Offset = offset,
                EventId = eventId
            };
        }
    }
}