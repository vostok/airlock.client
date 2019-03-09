namespace Vostok.Hercules.Client.Sink
{
    internal interface IStreamContextFactory
    {
        StreamContext Create(string streamName);
    }
}