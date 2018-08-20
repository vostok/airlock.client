using System;

namespace Vostok.Airlock.Client.Backoff
{
    internal interface IWithPreviousDelay : IWithDelay
    {
        IWithDelay WithDecorrelatedJitter(TimeSpan sendPeriodCap, TimeSpan sendPeriod);
    }
}