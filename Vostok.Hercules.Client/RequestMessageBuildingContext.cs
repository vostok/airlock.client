using System;
using System.Collections.Generic;

namespace Vostok.Hercules.Client
{
    internal class RequestMessageBuildingContext : IRequestMessageBuilder
    {
        private readonly RequestMessageBuilder builder;
        private readonly List<BufferSlice> slices;

        public RequestMessageBuildingContext(byte[] messageBuffer)
        {
            builder = new RequestMessageBuilder(messageBuffer);
            slices = new List<BufferSlice>();
        }

        public ArraySegment<byte> Message => builder.Message;

        public IRequestMessageBuilder Builder => this;

        public IReadOnlyList<BufferSlice> Slices => slices;

        bool IRequestMessageBuilder.TryAppend(BufferSlice slice)
        {
            if (!builder.TryAppend(slice))
                return false;

            slices.Add(slice);
            return true;
        }

        public bool IsFull => builder.IsFull;
    }
}