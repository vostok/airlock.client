using System;
using Vostok.Airlock.Client.Abstractions;
using Vostok.Commons.Binary;

namespace Vostok.Airlock.Client
{
    internal interface IAirlockRecordWriter
    {
        bool TryWrite(IBinaryWriter binaryWriter, Action<IAirlockRecordBuilder> build);
    }
}