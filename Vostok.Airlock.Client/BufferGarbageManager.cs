using System;
using System.Collections.Generic;

namespace Vostok.Airlock.Client
{
    internal static class BufferGarbageManager
    {
        public static void Join<T>(this List<T> sequentialGarbageSegments) where T : BufferGarbageSegment
        {
            for (var i = 0; i < sequentialGarbageSegments.Count - 1; i++)
            {
                var leftStartingPosition = sequentialGarbageSegments[i].Offset;
                var leftEndingPosition = leftStartingPosition + sequentialGarbageSegments[i].Length;

                var rightStartingPosition = sequentialGarbageSegments[i + 1].Offset;
                var rightEndingPosition = rightStartingPosition + sequentialGarbageSegments[i + 1].Length;

                if (leftEndingPosition > rightStartingPosition)
                {
                    throw new InvalidOperationException("Encountered intersecting garbage segments");
                }

                if (leftEndingPosition < rightStartingPosition)
                {
                    continue;
                }

                sequentialGarbageSegments[i + 1].Offset = leftStartingPosition;
                sequentialGarbageSegments[i + 1].Length = rightEndingPosition - leftStartingPosition;
                sequentialGarbageSegments[i + 1].RecordsCount += sequentialGarbageSegments[i].RecordsCount;

                sequentialGarbageSegments[i] = default;
            }

            var index = 0;
            for (var i = 0; i < sequentialGarbageSegments.Count; i++)
            {
                if (sequentialGarbageSegments[i] == default(T))
                {
                    continue;
                }

                sequentialGarbageSegments[index] = sequentialGarbageSegments[i];
                index++;
            }

            sequentialGarbageSegments.RemoveRange(index, sequentialGarbageSegments.Count - index);
        }
    }
}