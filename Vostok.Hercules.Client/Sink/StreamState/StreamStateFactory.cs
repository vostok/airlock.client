using System;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;

namespace Vostok.Hercules.Client.Sink.StreamState
{
    internal class StreamStateFactory : IStreamStateFactory
    {
        private const int InitialPooledBufferSize = 4096;

        private readonly HerculesSinkSettings settings;
        private readonly IMemoryManager memoryManager;

        public StreamStateFactory([NotNull] HerculesSinkSettings settings, [NotNull] IMemoryManager memoryManager)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
        }

        public IStreamState Create(string name)
            => new StreamState(name, CreateBufferPool(), new StatisticsCollector());

        private IBufferPool CreateBufferPool()
        {
            var privateMemoryManager = new MemoryManager(settings.MaximumPerStreamMemoryConsumption, memoryManager);

            return new BufferPool(
                privateMemoryManager,
                InitialPooledBufferSize,
                settings.MaximumRecordSize,
                settings.MaximumBatchSize);
        }
    }
}
