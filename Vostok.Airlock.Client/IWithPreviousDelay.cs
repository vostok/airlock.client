namespace Vostok.Airlock.Client
{
    internal interface IWithPreviousDelay : IWithDelay
    {
        IWithDelay WithDecorrelatedJitter(int sendPeriodCapMs, int sendPeriodMs);
    }
}