using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Sink.Sending;

namespace Vostok.Hercules.Client.Sink.Daemon
{
    internal class RecordsSendingJob : IRecordsSendingJob
    {
        private readonly IReadOnlyDictionary<string, Lazy<StreamContext>> streamContexts;
        private readonly WeakReference<IHerculesSink> sinkWeakReference;
        private readonly TimeSpan timeout;

        public RecordsSendingJob(
            IHerculesSink sink,
            IReadOnlyDictionary<string, Lazy<StreamContext>> streamContexts,
            TimeSpan timeout)
        {
            sinkWeakReference = new WeakReference<IHerculesSink>(sink);

            this.streamContexts = streamContexts;
            this.timeout = timeout;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var sendAny = false;

            foreach (var pair in streamContexts)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var streamStateLazy = pair.Value;

                if (!streamStateLazy.IsValueCreated)
                    continue;

                var streamState = streamStateLazy.Value;

                if (!streamState.Sender.Signal.IsCompleted)
                    continue;

                var sendingResult = await streamState.Sender.SendAsync(timeout, cancellationToken).ConfigureAwait(false);

                if (sendingResult == SendResult.Success)
                    sendAny = true;
            }

            if (!sendAny && SinkIsCollectedByGC())
                throw new OperationCanceledException();
        }

        public Task WaitNextOccurrenceAsync()
        {
            var delays = streamContexts
                .Select(x => x.Value)
                .Where(x => x.IsValueCreated)
                .Select(x => x.Value.Sender.Signal);

            return Task.WhenAny(delays);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SinkIsCollectedByGC()
        {
            return !sinkWeakReference.TryGetTarget(out _);
        }
    }
}