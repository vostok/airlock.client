using System;
using Vostok.Airlock.Client.Abstractions;
using Vostok.Airlock.Client.Binary;

namespace Vostok.Airlock.Client
{
    internal interface IAirlockRecordsWriter
    {
        bool TryWrite(IBinaryWriter binaryWriter, Action<IAirlockRecordBuilder> build);
    }
}