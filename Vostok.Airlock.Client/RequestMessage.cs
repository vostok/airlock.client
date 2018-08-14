using System;
using System.Collections.Generic;

namespace Vostok.Airlock.Client
{
    internal class RequestMessage
    {
        public ArraySegment<byte> Message { get; set; }
        public IReadOnlyCollection<BufferSlice> ParticipatingSlices { get; set; }
        public int RecordsCount { get; set; }
    }
}