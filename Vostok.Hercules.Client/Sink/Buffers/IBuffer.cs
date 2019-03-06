using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IBuffer : IBinaryWriter
    {
        bool IsOverflowed { get; set; }
        void Commit(int recordSize);
        BufferState GetState();
        bool IsEmpty();
        BufferSnapshot MakeSnapshot();
        void CollectGarbage();
        void RequestGarbageCollection(BufferState state);
        bool HasGarbage();
    }
}