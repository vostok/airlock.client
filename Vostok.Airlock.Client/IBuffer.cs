using Vostok.Commons.Binary;

namespace Vostok.Airlock.Client
{
    internal interface IBuffer
    {
        int CommitedPosition { get; }
        int WrittenRecords { get; }

        IBinaryWriter BeginRecord();
        void Commit();
    }
}