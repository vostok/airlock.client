using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Airlock.Client
{
    internal class Schedule : ISchedule
    {
        private readonly TimeSpan delayToNextOccurrence;

        public Schedule(TimeSpan delayToNextOccurrence)
        {
            this.delayToNextOccurrence = delayToNextOccurrence;
        }

        public async Task WaitNextOccurrenceAsync(CancellationToken cancellationToken = default)
        {
            if (delayToNextOccurrence > TimeSpan.Zero)
                await Task.Delay(delayToNextOccurrence, cancellationToken).ConfigureAwait(false);
        }
    }
}