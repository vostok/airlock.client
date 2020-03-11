using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Models;

namespace Vostok.Hercules.Client.Helpers
{
    internal static class StreamCoordinatesHelper
    {
        /// <summary>
        /// Add zeros to left, filter by right partitions.
        /// </summary>
        public static StreamCoordinates FixQueryCoordinates([NotNull] StreamCoordinates left, [NotNull] StreamCoordinates right)
        {
            var map = left.ToDictionary();
            var result = new List<StreamPosition>();

            foreach (var position in right.Positions)
            {
                if (!map.TryGetValue(position.Partition, out var inital))
                {
                    result.Add(
                        new StreamPosition
                        {
                            Offset = 0,
                            Partition = position.Partition
                        });
                }
                else
                {
                    result.Add(inital);
                }
            }

            return new StreamCoordinates(result.ToArray());
        }
    }
}