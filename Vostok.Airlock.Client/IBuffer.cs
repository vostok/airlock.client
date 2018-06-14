using Vostok.Commons.Binary;

namespace Vostok.Airlock.Client
{
    internal interface IBuffer
    {
        IBinaryWriter BeginRecord();
        void Commit();

        void CollectGarbage();
        void RequestGarbageCollection();

        BufferSnapshot MakeSnapshot();

        bool IsEmpty();
    }
}