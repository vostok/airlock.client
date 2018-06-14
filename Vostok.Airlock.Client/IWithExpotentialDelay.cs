namespace Vostok.Airlock.Client
{
    internal interface IWithExpotentialDelay : IWithDelay
    {
        IWithDelay WithFullJitter();
        IWithDelay WithEqualJitter();
    }
}