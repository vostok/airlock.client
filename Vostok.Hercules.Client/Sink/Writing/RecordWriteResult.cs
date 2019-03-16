namespace Vostok.Hercules.Client.Sink.Writing
{
    internal enum RecordWriteResult
    {
        /// <summary>
        /// Record has been written successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Record exceeded maximum allowed size.
        /// </summary>
        RecordTooLarge,

        /// <summary>
        /// Record has not been written due to memory shortage.
        /// </summary>
        OutOfMemory,

        /// <summary>
        /// Record has not been written due to unexpected exception.
        /// </summary>
        Exception
    }
}