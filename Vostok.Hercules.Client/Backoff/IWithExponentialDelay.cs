namespace Vostok.Hercules.Client.Backoff
{
    internal interface IWithExponentialDelay : IWithDelay
    {
        IWithDelay WithFullJitter();
        IWithDelay WithEqualJitter();
    }
}