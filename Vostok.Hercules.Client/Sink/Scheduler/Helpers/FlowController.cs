using System;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal class FlowController : IFlowController
    {
        private readonly WeakReference ownerSinkReference;

        public FlowController(WeakReference ownerSinkReference)
        {
            this.ownerSinkReference = ownerSinkReference;
        }

        public bool ShouldStillOperateOn(SchedulerState state)
            => !state.CancellationToken.IsCancellationRequested && ownerSinkReference.IsAlive;
    }
}