using System;

namespace Vostok.Airlock.Client
{
    internal interface IWithPreviousDelay : IWithDelay
    {
        IWithDelay WithDecorrelatedJitter(TimeSpan sendPeriodCap, TimeSpan sendPeriod);
    }
}