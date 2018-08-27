using System;
using System.Collections.Concurrent;
using System.Threading;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    public class HerculesGateClient : IHerculesGateClient, IDisposable
    {
        private readonly IHerculesRecordWriter recordWriter;
        private readonly IMemoryManager memoryManager;

        private readonly int initialPooledBuffersCount;
        private readonly int initialPooledBufferSize;
        private readonly ConcurrentDictionary<string, Lazy<IBufferPool>> bufferPools;

        private readonly HerculesRecordsSendingDaemon recordsSendingDaemon;

        private int isDisposed;
        private int lostRecordsCounter;

        public HerculesGateClient(HerculesConfig config)
        {
            var log = new SilentLog();
            recordWriter = new HerculesRecordWriter(log, config.RecordVersion, (int) config.MaximumRecordSize.Bytes);

            memoryManager = new MemoryManager(config.MaximumMemoryConsumption.Bytes);

            initialPooledBuffersCount = config.InitialPooledBuffersCount;
            initialPooledBufferSize = (int) config.InitialPooledBufferSize.Bytes;
            bufferPools = new ConcurrentDictionary<string, Lazy<IBufferPool>>();

            var jobScheduler = new HerculesRecordsSendingJobScheduler(memoryManager, config.RequestSendPeriod, config.RequestSendPeriodCap);
            var bufferSlicer = new BufferSliceFactory((int) config.MaximumRequestContentSize.Bytes - sizeof(int));
            var messageBuffer = new byte[config.MaximumRequestContentSize.Bytes];
            var requestSender = new RequestSender(log, config.GateName, config.GateUri, config.GateApiKey, config.RequestTimeout);
            var job = new HerculesRecordsSendingJob(log, jobScheduler, bufferPools, bufferSlicer, messageBuffer, requestSender);
            recordsSendingDaemon = new HerculesRecordsSendingDaemon(log, job);
        }

        public int LostRecordsCount =>
            Interlocked.Add(ref lostRecordsCounter, recordsSendingDaemon.LostRecordsCount);

        public int SentRecordsCount =>
            recordsSendingDaemon.SentRecordsCount;

        public void Put(string stream, Action<IHerculesRecordBuilder> build)
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