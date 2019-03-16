using System;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Writing
{
    internal interface IRecordWriter
    {
        /// <summary>
        /// <para>Attempts to write a record to given <paramref name="buffer"/> using supplied <paramref name="build"/> delegate.</para>
        /// <para>Commits written record with <see cref="IBuffer.CommitRecord"/> in case of success.</para>
        /// <para>Rolls buffer position back to its starting value if any error occurs along the way.</para>
        /// </summary>
        RecordWriteResult TryWrite([NotNull] IBuffer buffer, [NotNull] Action<IHerculesEventBuilder> build, out int recordSize);
    }
}