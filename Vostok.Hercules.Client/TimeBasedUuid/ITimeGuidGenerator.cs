namespace Vostok.Hercules.Client.TimeBasedUuid
{
    internal interface ITimeGuidGenerator
    {
        TimeGuid NewGuid();
        TimeGuid NewGuid(long timestamp);
    }
}