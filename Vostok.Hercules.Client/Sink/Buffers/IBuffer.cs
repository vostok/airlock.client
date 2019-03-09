using JetBrains.Annotations;
using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IBuffer : IBinaryWriter
    {
        bool IsOverflowed { get; set; }
        void Commit(int recordSize);
        BufferState GetState();

        [CanBeNull]
        BufferSnapshot TryMakeSnapshot();

        void CollectGarbage();
        void RequestGarbageCollection(BufferState state);
    }
}