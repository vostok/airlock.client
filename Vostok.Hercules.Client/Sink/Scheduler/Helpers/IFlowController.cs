namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal interface IFlowController
    {
        bool ShouldStillOperateOn(SchedulerState state);
    }
}