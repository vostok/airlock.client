using System.Collections.Generic;

namespace Vostok.Airlock.Client
{
    internal class RequestMessageBuildingContext : IBufferSliceAppender
    {
        private readonly RequestMessageBuilder builder;
        private readonly List<BufferSlice> slices;

        private int recordsCounter;

        public RequestMessageBuildingContext(byte[] messageBuffer)
        {
            builder = new RequestMessageBuilder(messageBuffer);
            slices = new List<BufferSlice>();
        }

        public IBufferSliceAppender Appender => this;

        bool IBufferSliceAppender.TryAppend(BufferSlice slice)
        {
            if (!builder.TryAppend(slice))
            {
                return false;
            }

            slices.Add(slice);
            recordsCounter += slice.Count;
            return true;
        }

        public RequestMessage Build()
        {
            builder.WriteRecordsCount(recordsCounter);

            return new RequestMessage {Message = builder.Message, ParticipatingSlices = slices, RecordsCount = recordsCounter};
        }

        public void Reset()
        {
            builder.Reset();
            slices.Clear();
            recordsCounter = 0;
        }
    }
}