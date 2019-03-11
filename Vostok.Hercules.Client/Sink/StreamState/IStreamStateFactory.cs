namespace Vostok.Hercules.Client.Sink.StreamState
{
    internal interface IStreamStateFactory
    {
        IStreamState Create(string streamName);
    }
}