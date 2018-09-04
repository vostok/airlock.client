using System;
using System.Collections.Generic;

namespace Vostok.Hercules.Client
{
    internal static class DefragmentationManager
    {
        public static int Run<T>(ArraySegment<byte> source, IReadOnlyList<T> sequentialGarbageSegments)
            where T : ILineSegment
        {
            var currentPosition = sequentialGarbageSegments[0].Offset;

            for (var i = 0; i < sequentialGarbageSegments.Count; i++)
            {
                var garbageBytesStartingPosition = currentPosition;
                var garbageBytesCount = sequentialGarbageSegments[i].Offset - currentPosition + sequentialGarbageSegments[i].Length;

                var usefulBytesStartingPosition = garbageBytesStartingPosition + garbageBytesCount;
                var usefulBytesEndingPosition = sequentialGarbageSegments.HasNext(i) ? sequentialGarbageSegments[i + 1].Offset : source.Offset + source.Count;

                var usefulBytesCount = usefulBytesEndingPosition - usefulBytesStartingPosition;

                System.Buffer.BlockCopy(source.Array, usefulBytesStartingPosition, source.Array, garbageBytesStartingPosition, usefulBytesCount);

                currentPosition = garbageBytesStartingPosition + usefulBytesCount;
            }

            return currentPosition;
        }

        private static bool HasNext<T>(this IReadOnlyCollection<T> source, int currentIndex) => 
            source.Count - 1 != currentIndex;
    }
}