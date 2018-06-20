using System;

namespace Vostok.Airlock.Client
{
    internal interface IWithDelay
    {
        TimeSpan Value { get; }
    }
}