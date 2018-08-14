namespace Vostok.Airlock.Client
{
    internal interface IWithExponentialDelay : IWithDelay
    {
        IWithDelay WithFullJitter();
        IWithDelay WithEqualJitter();
    }
}