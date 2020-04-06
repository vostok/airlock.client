using System;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Hercules.Client.Abstractions.Models;

namespace Vostok.Hercules.Client.Internal
{
    internal class RawReadStreamPayload : IDisposable
    {
        private readonly ValueDisposable<ArraySegment<byte>> content;

        public RawReadStreamPayload(ValueDisposable<ArraySegment<byte>> content, StreamCoordinates next)
        {
            this.content = content;
            Content = content.Value;
            Next = next;
        }

        public ArraySegment<byte> Content { get; }

        public StreamCoordinates Next { get; }

        public void Dispose() => content?.Dispose();
    }
}