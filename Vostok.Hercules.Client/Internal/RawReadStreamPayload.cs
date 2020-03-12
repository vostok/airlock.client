using System;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Hercules.Client.Abstractions.Models;

namespace Vostok.Hercules.Client.Internal
{
    internal class RawReadStreamPayload : IDisposable
    {
        private readonly ValueDisposable<ArraySegment<byte>> content;
        
        public RawReadStreamPayload(ValueDisposable<ArraySegment<byte>> content, StreamCoordinates coordinates)
        {
            this.content = content;
            Content = content.Value;
            Coordinates = coordinates;
        }
        
        public ArraySegment<byte> Content { get; }

        public StreamCoordinates Coordinates { get; }

        public void Dispose() => content?.Dispose();
    }
}