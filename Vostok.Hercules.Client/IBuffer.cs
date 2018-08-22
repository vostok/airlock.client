using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal interface IBuffer
    {
        IBinaryWriter BeginRecord();
        void Commit();
        void CollectGarbage();
        void RequestGarbageCollection(int offset, int length, int recordsCount);
        BufferSnapshot MakeSnapshot();
        bool IsEmpty();
    }
}