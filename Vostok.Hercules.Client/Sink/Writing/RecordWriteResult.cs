namespace Vostok.Hercules.Client.Sink.Writing
{
    internal enum RecordWriteResult
    {
        Success,
        RecordTooLarge,
        OutOfMemory,
        Exception
    }
}