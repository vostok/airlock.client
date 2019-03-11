using System;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Writing
{
    internal interface IRecordWriter
    {
        WriteResult TryWrite(IBuffer binaryWriter, Action<IHerculesEventBuilder> build, out int recordSize);
    }
}