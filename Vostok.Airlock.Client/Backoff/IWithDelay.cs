using System;

namespace Vostok.Airlock.Client.Backoff
{
    internal interface IWithDelay
    {
        TimeSpan Value { get; }
    }
}