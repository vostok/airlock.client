using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal interface IBuffer
    {
        IHerculesBinaryWriter BeginRecord();
        void Commit(int recordSize);
        int EstimateRecordsCountForMonitoring();
        bool IsEmpty();
        BufferSnapshot MakeSnapshot();
        void CollectGarbage();
        void RequestGarbageCollection(BufferState state);
        bool TryLock();
        void Unlock();
        bool HasGarbage();
    }
}