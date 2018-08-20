namespace Vostok.Airlock.Client.Backoff
{
    internal interface IWithExponentialDelay : IWithDelay
    {
        IWithDelay WithFullJitter();
        IWithDelay WithEqualJitter();
    }
}