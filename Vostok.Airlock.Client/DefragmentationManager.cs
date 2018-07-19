using System.Collections.Generic;

namespace Vostok.Airlock.Client
{
    internal static class DefragmentationManager
    {
        public static int Run<T>(byte[] buffer, IReadOnlyList<T> sequentialGarbageSegments)
            where T : ILineSegment
        {
            var currentPosition = sequentialGarbageSegments[0].Offset;

            for (var i = 0; i < sequentialGarbageSegments.Count; i++)
            {
                var garbageBytesStartingPosition = currentPosition;
                var garbageBytesCount = sequentialGarbageSegments[i].Offset - currentPosition + sequentialGarbageSegments[i].Length;

                var usefulBytesStartingPosition = garbageBytesStartingPosition + garbageBytesCount;
                var usefulBytesEndingPosition = i != sequentialGarbageSegments.Count - 1 ? sequentialGarbageSegments[i + 1].Offset : buffer.Length;

                var usefulBytesCount = usefulBytesEndingPosition - usefulBytesStartingPosition;

                System.Buffer.BlockCopy(buffer, usefulBytesStartingPosition, buffer, garbageBytesStartingPosition, usefulBytesCount);

                currentPosition = garbageBytesStartingPosition + usefulBytesCount;
            }

            return currentPosition;
        }
    }
}