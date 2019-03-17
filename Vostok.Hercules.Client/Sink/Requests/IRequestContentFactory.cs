using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Requests
{
    internal interface IRequestContentFactory
    {
        [NotNull]
        CompositeContent CreateContent([NotNull] IReadOnlyList<BufferSnapshot> snapshots, out int recordsCount, out int recordsSize);
    }
}