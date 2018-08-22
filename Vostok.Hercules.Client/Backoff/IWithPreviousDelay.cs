using System;

namespace Vostok.Hercules.Client.Backoff
{
    internal interface IWithPreviousDelay : IWithDelay
    {
        IWithDelay WithDecorrelatedJitter(TimeSpan sendPeriodCap, TimeSpan sendPeriod);
    }
}