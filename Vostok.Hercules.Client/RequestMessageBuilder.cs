using System;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal class RequestMessageBuilder : IRequestMessageBuilder
    {
        private readonly BinaryBufferWriter writer;

        private int recordsCounter;

        public RequestMessageBuilder(byte[] buffer)
        {
            writer = new BinaryBufferWriter(buffer) {Position = sizeof(int)};
        }

        public ArraySegment<byte> Message => writer.FilledSegment;
        
        public bool IsFull => writer.Buffer.Length - writer.Position == 0;

        public bool TryAppend(BufferSlice slice)
        {
            if (!IsFit(slice))
                return false;

            writer.Write(slice.Buffer, slice.Offset, slice.Length);

            recordsCounter += slice.RecordsCount;

            var positionBefore = writer.Position;
            writer.Position = 0;
            writer.WriteInNetworkByteOrder(recordsCounter);
            writer.Position = positionBefore;

            return true;
        }

        private bool IsFit(BufferSlice slice)
        {
            var remaining = writer.Buffer.Length - writer.Position;

            if (slice.Length <= remaining)
                return true;

            // TODO: why is it? this exception can crash entire sending daemon forever
            if (recordsCounter == 0)
                throw new Exception($"Buffer slice of size {slice.Length} does not fit into maximum message size {writer.Buffer.Length}");

            return false;
        }
    }
}