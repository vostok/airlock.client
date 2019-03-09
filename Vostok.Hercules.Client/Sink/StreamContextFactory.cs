using Vostok.Commons.Primitives;
using Vostok.Hercules.Client.Gateway;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.Sending;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink
{
    internal class StreamContextFactory : IStreamContextFactory
    {
        private const int InitialPooledBufferSize = 4 * (int)DataSizeConstants.Kilobyte;

        private readonly HerculesSinkSettings settings;
        private readonly IBufferSnapshotBatcher batcher;
        private readonly IRequestContentFactory contentFactory;
        private readonly IRequestSender requestSender;
        private readonly IMemoryManager memoryManager;
        private readonly ILog log;

        public StreamContextFactory(
            HerculesSinkSettings settings,
            IBufferSnapshotBatcher batcher,
            IRequestContentFactory contentFactory,
            IRequestSender requestSender,
            IMemoryManager memoryManager,
            ILog log)
        {
            this.settings = settings;
            this.batcher = batcher;
            this.contentFactory = contentFactory;
            this.requestSender = requestSender;
            this.memoryManager = memoryManager;
            this.log = log;
        }

        public StreamContext Create(string streamName)
        {
            var statisticsCollector = new StatisticsCollector();
            var streamState = new StreamState(streamName, CreateBufferPool(), statisticsCollector);
            var sender = new StreamSender(streamState, batcher, contentFactory, requestSender, log);
            var schedulingSender = new SchedulingStreamSender(sender, settings.RequestSendPeriod, settings.RequestSendPeriodCap);

            return new StreamContext(schedulingSender, streamState);
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