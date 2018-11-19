using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal interface IBuffer
    {
        IBinaryWriter BeginRecord();
        void Commit(int recordSize);
        int GetRecordSize(int offset);
        int EstimateRecordsCountForMonitoring();
        bool IsEmpty();
        BufferSnapshot MakeSnapshot();
        void CollectGarbage();
        void RequestGarbageCollection(int offset, int length, int recordsCount);
    }
}