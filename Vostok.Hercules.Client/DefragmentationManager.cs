using System;
using System.Collections.Generic;

namespace Vostok.Hercules.Client
{
    internal static class DefragmentationManager
    {
        public static int Run<T>(ArraySegment<byte> segment, IReadOnlyList<T> sequentialGarbageSegments)
            where T : ILineSegment
        {
            if (segment.Array == null)
                throw new ArgumentNullException(nameof(segment));

            if (sequentialGarbageSegments == null)
                throw new ArgumentNullException(nameof(sequentialGarbageSegments));

            var currentPosition = sequentialGarbageSegments[0].Offset;

            for (var i = 0; i < sequentialGarbageSegments.Count; i++)
            {
                var garbageBytesStartingPosition = currentPosition;
                var garbageBytesCount = sequentialGarbageSegments[i].Offset - currentPosition + sequentialGarbageSegments[i].Length;

                var usefulBytesStartingPosition = garbageBytesStartingPosition + garbageBytesCount;
                var usefulBytesEndingPosition = sequentialGarbageSegments.HasNext(i) ? sequentialGarbageSegments[i + 1].Offset : segment.Offset + segment.Count;

                var usefulBytesCount = usefulBytesEndingPosition - usefulBytesStartingPosition;

                System.Buffer.BlockCopy(segment.Array, usefulBytesStartingPosition, segment.Array, garbageBytesStartingPosition, usefulBytesCount);

                currentPosition = garbageBytesStartingPosition + usefulBytesCount;
            }

            return currentPosition;
        }

        private static bool HasNext<T>(this IReadOnlyCollection<T> segments, int currentIndex) => 
            segments.Count - 1 != currentIndex;
    }
}