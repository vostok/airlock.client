using JetBrains.Annotations;
using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IBuffer : IBinaryWriter
    {
        /// <summary>
        /// Gets or sets whether this buffer has reached its maximum size and can no longer be written to.
        /// </summary>
        bool IsOverflowed { get; set; }

        /// <summary>
        /// Returns total current size of committed records excepts garbage reported with <see cref="RequestGarbageCollection"/>.
        /// </summary>
        long UsefulDataSize { get; }

        /// <summary>
        /// Commits a recently written record of given <paramref name="size"/>, so that it will be included in result of the next successful <see cref="TryMakeSnapshot"/> call.
        /// </summary>
        void CommitRecord(int size);

        [CanBeNull]
        BufferSnapshot TryMakeSnapshot();

        void CollectGarbage();
        void RequestGarbageCollection(BufferState state);
    }
}
