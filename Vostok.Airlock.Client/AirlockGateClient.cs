using System;
using System.Collections.Concurrent;
using System.Threading;
using Vostok.Airlock.Client.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Airlock.Client
{
    public class AirlockGateClient : IAirlockGateClient, IDisposable
    {
        private readonly IAirlockRecordWriter recordWriter;
        private readonly IMemoryManager memoryManager;

        private readonly int initialPooledBuffersCount;
        private readonly int initialPooledBufferSize;
        private readonly ConcurrentDictionary<string, Lazy<IBufferPool>> bufferPools;

        private readonly AirlockRecordsSendingDaemon recordsSendingDaemon;

        private int isDisposed;
        private int lostRecordsCounter;

        public AirlockGateClient(ILog log, AirlockConfig config)
        {
            recordWriter = new AirlockRecordWriter(log, (int)config.MaximumRecordSize.Bytes);

            memoryManager = new MemoryManager(config.MaximumMemoryConsumption.Bytes);

            initialPooledBuffersCount = config.InitialPooledBuffersCount;
            initialPooledBufferSize = (int)config.InitialPooledBufferSize.Bytes;
            bufferPools = new ConcurrentDictionary<string, Lazy<IBufferPool>>();

            var jobScheduler = new AirlockRecordsSendingJobScheduler(memoryManager, config.RequestSendPeriod, config.RequestSendPeriodCap);
            var bufferSlicer = new BufferSliceFactory((int)config.MaximumRequestContentSize.Bytes - sizeof(int));
            var messageBuffer = new byte[config.MaximumRequestContentSize.Bytes];
            var requestSender = new RequestSender(log, config.GateName, config.GateUri, config.GateApiKey, config.RequestTimeout);
            var job = new AirlockRecordsSendingJob(log, jobScheduler, bufferPools, bufferSlicer, messageBuffer, requestSender);
            recordsSendingDaemon = new AirlockRecordsSendingDaemon(log, job);
        }

        public int LostRecordsCount => Interlocked.Add(ref lostRecordsCounter, recordsSendingDaemon.LostRecordsCount);

        public int SentRecordsCount => recordsSendingDaemon.SentRecordsCount;

        public void Put(string stream, Action<IAirlockRecordBuilder> build)
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

                if (recordWriter.TryWrite(binaryWriter, build))
                    buffer.Commit();
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