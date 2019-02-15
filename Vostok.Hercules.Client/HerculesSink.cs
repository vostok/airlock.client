using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Vostok.Commons.Primitives;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc cref="IHerculesSink" />
    public class HerculesSink : IHerculesSink, IDisposable
    {
        private const int RecordVersion = 1;
        private const int InitialPooledBuffersCount = 1;
        private const int InitialPooledBufferSize = 4 * (int) DataSizeConstants.Kilobyte;
        
        private readonly ILog log;

        private readonly IHerculesRecordWriter recordWriter;
        private readonly IMemoryManager memoryManager;

        private readonly int initialPooledBuffersCount;
        private readonly int initialPooledBufferSize;
        private readonly ConcurrentDictionary<string, Lazy<IBufferPool>> bufferPools;

        private readonly HerculesRecordsSendingDaemon recordsSendingDaemon;

        private int isDisposed;
        private long lostRecordsCounter;
        private int maxRecordSize;
        private int maxRequestBodySize;
        private long maximumPerStreamMemoryConsumptionBytes;

        /// <summary>
        /// Creates a new instance of <see cref="HerculesSink"/>.
        /// </summary>
        /// <param name="config">Configuration of this <see cref="HerculesSink"/></param>
        /// <param name="log">A <see cref="ILog"/> instance.</param>
        public HerculesSink(HerculesSinkConfig config, ILog log)
        {
            log = (log ?? LogProvider.Get()).ForContext<HerculesSink>();

            recordWriter = new HerculesRecordWriter(log, () => PreciseDateTime.UtcNow, RecordVersion, config.MaximumRecordSize);

            memoryManager = new MemoryManager(config.MaximumMemoryConsumption);

            initialPooledBuffersCount = InitialPooledBuffersCount;
            initialPooledBufferSize = InitialPooledBufferSize;
            maxRecordSize = config.MaximumRecordSize;
            maxRequestBodySize = config.MaximumBatchSize;
            maximumPerStreamMemoryConsumptionBytes = config.MaximumPerStreamMemoryConsumption;
            bufferPools = new ConcurrentDictionary<string, Lazy<IBufferPool>>();

            var jobScheduler = new HerculesRecordsSendingJobScheduler(
                memoryManager,
                config.RequestSendPeriod,
                config.RequestSendPeriodCap);

            var requestSender = new RequestSender(log, config);
            
            var job = new HerculesRecordsSendingJob(
                bufferPools,
                jobScheduler,
                requestSender,
                log,
                config.RequestTimeout);
            
            recordsSendingDaemon = new HerculesRecordsSendingDaemon(log, job);
        }

        /// <summary>
        /// How many records are lost due to memory limit violation and network communication errors.
        /// </summary>
        public long LostRecordsCount => Interlocked.Read(ref lostRecordsCounter) + recordsSendingDaemon.LostRecordsCount;

        /// <summary>
        /// How many records already sent with this <see cref="HerculesSink"/>.
        /// </summary>
        public long SentRecordsCount =>
            recordsSendingDaemon.SentRecordsCount;

        /// <summary>
        /// How many records stored inside <see cref="HerculesSink"/> internal buffers and waiting to be sent.
        /// </summary>
        public long StoredRecordsCount =>
            bufferPools
                .Where(x => x.Value.IsValueCreated)
                .Select(x => x.Value.Value)
                .Sum(x => x.GetStoredRecordsCount());

        /// <inheritdoc />
        public void Put(string stream, Action<IHerculesEventBuilder> build)
        {
            var bufferPool = GetOrCreate(stream);

            if (!bufferPool.TryAcquire(out var buffer))
            {
                Interlocked.Increment(ref lostRecordsCounter);
                return;
            }

            try
            {
                var binaryWriter = buffer.BeginRecord();

                if (recordWriter.TryWrite(binaryWriter, build, out var recordSize))
                    buffer.Commit(recordSize);
                else
                    Interlocked.Increment(ref lostRecordsCounter);
            }
            finally
            {
                bufferPool.Release(buffer);
            }

            recordsSendingDaemon.Initialize();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 1)
                recordsSendingDaemon.Dispose();
        }

        private IBufferPool GetOrCreate(string stream) =>
            bufferPools.GetOrAdd(stream, _ => new Lazy<IBufferPool>(CreateBufferPool, LazyThreadSafetyMode.ExecutionAndPublication)).Value;

        private IBufferPool CreateBufferPool()
        {
            var perStreamMemoryManager = new MemoryManager(maximumPerStreamMemoryConsumptionBytes, memoryManager);

            return new BufferPool(
                perStreamMemoryManager,
                initialPooledBuffersCount,
                initialPooledBufferSize,
                maxRecordSize,
                maxRequestBodySize);
        }
    }
}