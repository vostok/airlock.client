﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.TimeBasedUuid;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    public class HerculesGateClient : IHerculesGateClient, IDisposable
    {
        private readonly ILog log = new SilentLog();

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
            recordWriter = new HerculesRecordWriter(log, new TimeGuidGenerator(), config.RecordVersion, (int) config.MaximumRecordSizeBytes);

            memoryManager = new MemoryManager(config.MaximumMemoryConsumptionBytes);

            initialPooledBuffersCount = config.InitialPooledBuffersCount;
            initialPooledBufferSize = (int) config.InitialPooledBufferSizeBytes;
            bufferPools = new ConcurrentDictionary<string, Lazy<IBufferPool>>();

            var jobScheduler = new HerculesRecordsSendingJobScheduler(memoryManager, config.RequestSendPeriod, config.RequestSendPeriodCap);
            var bufferSlicer = new BufferSliceFactory((int) config.MaximumRequestContentSizeBytes - sizeof(int));
            var messageBuffer = new byte[config.MaximumRequestContentSizeBytes];
            var requestSender = new RequestSender(log, config.GateName, config.GateUri, config.GateApiKey, config.RequestTimeout);
            var job = new HerculesRecordsSendingJob(log, jobScheduler, bufferPools, bufferSlicer, messageBuffer, requestSender);
            recordsSendingDaemon = new HerculesRecordsSendingDaemon(log, job);
        }

        public int LostRecordsCount => lostRecordsCounter + recordsSendingDaemon.LostRecordsCount;

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