namespace Vostok.Airlock.Client
{
    internal interface IBufferSliceAppender
    {
        bool TryAppend(BufferSlice slice);
    }
}   