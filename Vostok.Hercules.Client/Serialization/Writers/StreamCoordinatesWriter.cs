using JetBrains.Annotations;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Serialization.Helpers;

namespace Vostok.Hercules.Client.Serialization.Writers
{
    internal static class StreamCoordinatesWriter
    {
        public static void Write([NotNull] StreamCoordinates coordinates, [NotNull] IBinaryWriter writer)
        {
            writer.EnsureBigEndian();

            writer.Write(coordinates.Positions.Length);

            foreach (var position in coordinates.Positions)
            {
                writer.Write(position.Partition);
                writer.Write(position.Offset);
            }
        }
    }
}
