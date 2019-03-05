using System;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client.Sink.Writing
{
    internal interface IHerculesRecordWriter
    {
        bool TryWrite(IHerculesBinaryWriter binaryWriter, Action<IHerculesEventBuilder> build, out int recordSize);
    }
}