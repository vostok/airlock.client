using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    public class HerculesSink : IHerculesSink, IDisposable
    {
        private readonly ILog log;

        private readonly IHerculesRecordWriter recordWriter;
        private readonly IMemoryManager memoryManager;

        private readonly int initialPooledBuffersCount;
        private readonly int initialPooledBufferSize;
        private readonly ConcurrentDictionary<string, Lazy<IBufferPool>> bufferPools;

        private readonly HerculesRecordsSendingDaemon recordsSendingDaemon;

        private int isDisposed;
        private long lostRecordsCounter;

        public HerculesSink(HerculesSinkConfig config, ILog log)
        {
            log = log ?? new SilentLog();
            
            recordWriter = new HerculesRecordWriter(log, () => PreciseDateTime.UtcNow, config.RecordVersion, (int) config.MaximumRecordSizeBytes);

            memoryManager = new MemoryManager(config.MaximumMemoryConsumptionBytes);

            initialPooledBuffersCount = config.InitialPooledBuffersCount;
            initialPooledBufferSize = (int) config.InitialPooledBufferSizeBytes;
            bufferPools = new ConcurrentDictionary<string, Lazy<IBufferPool>>();

            var jobScheduler = new HerculesRecordsSendingJobScheduler(memoryManager, config.RequestSendPeriod, config.RequestSendPeriodCap);
            var bufferSlicer = new BufferSliceFactory((int) config.MaximumRequestContentSizeBytes - sizeof(int));
            var messageBuffer = new byte[config.MaximumRequestContentSizeBytes];
            var requestSender = new RequestSender(log, config);
            var job = new HerculesRecordsSendingJob(log, jobScheduler, bufferPools, bufferSlicer, messageBuffer, requestSender, config.RequestTimeout);
            recordsSendingDaemon = new HerculesRecordsSendingDaemon(log, job);
        }

        public long LostRecordsCount => Interlocked.Read(ref lostRecordsCounter) + recordsSendingDaemon.LostRecordsCount;

        public long SentRecordsCount =>
            recordsSendingDaemon.SentRecordsCount;

        public long StoredRecordsCount =>
            bufferPools
                .Where(x => x.Value.IsValueCreated)
                .Select(x => x.Value.Value)
                .Sum(x => x.GetStoredRecordsCount());

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
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 1)
                recordsSendingDaemon.Dispose();
        }

        private IBufferPool GetOrCreate(string stream) =>
            bufferPools.GetOrAdd(stream, _ => new Lazy<IBufferPool>(CreateBufferPool, LazyThreadSafetyMode.ExecutionAndPublication)).Value;

        private IBufferPool CreateBufferPool() =>
            new BufferPool(memoryManager, initialPooledBuffersCount, initialPooledBufferSize);
    }
}