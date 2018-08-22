using System;

namespace Vostok.Hercules.Client.Backoff
{
    internal interface IWithDelay
    {
        TimeSpan Value { get; }
    }
}