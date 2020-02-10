using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Requests
{
    internal interface IRequestContentFactory
    {
        [NotNull]
        ValueDisposable<Content> CreateContent([NotNull] IReadOnlyList<BufferSnapshot> snapshots, out int recordsCount, out int recordsSize);
    }
}