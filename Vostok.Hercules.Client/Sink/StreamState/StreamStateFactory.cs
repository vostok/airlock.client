using Vostok.Commons.Primitives;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;

namespace Vostok.Hercules.Client.Sink.StreamState
{
    internal class StreamStateFactory : IStreamStateFactory
    {
        private const int InitialPooledBufferSize = 4 * (int)DataSizeConstants.Kilobyte;

        private readonly HerculesSinkSettings settings;
        private readonly IMemoryManager memoryManager;

        public StreamStateFactory(
            HerculesSinkSettings settings,
            IMemoryManager memoryManager)
        {
            this.settings = settings;
            this.memoryManager = memoryManager;
        }

        public IStreamState Create(string streamName)
        {
            var statisticsCollector = new StatisticsCollector();
            return new StreamState(streamName, CreateBufferPool(), statisticsCollector);
        }

        private IBufferPool CreateBufferPool()
        {
            var perStreamMemoryManager = new MemoryManager(settings.MaximumPerStreamMemoryConsumption, memoryManager);

            return new BufferPool(
                perStreamMemoryManager,
                InitialPooledBufferSize,
                settings.MaximumRecordSize,
                settings.MaximumBatchSize);
        }
    }
}