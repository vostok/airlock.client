namespace Vostok.Hercules.Client.Sink.Writing
{
    internal enum WriteResult
    {
        NoError,
        RecordTooLarge,
        OutOfMemory,
        Exception
    }
}