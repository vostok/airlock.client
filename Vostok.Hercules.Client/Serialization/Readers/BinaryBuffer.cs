using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client.Serialization.Readers
{
    internal class BinaryBuffer : IBinaryBuffer
    {
        public BinaryBufferReader Reader;

        public byte[] Buffer { get; private set; }

        public long Position
        {
            get { return Reader.Position; }
            set { Reader.Position = value; }
        }

        public bool SkipMode
        {
            get { return Reader.SkipMode; }
            set { Reader.SkipMode = value; }
        }
        
        public BinaryBuffer(byte[] bytes, long offset)
        {
            Buffer = bytes;

            Reader = new BinaryBufferReader(bytes, offset)
            {
                Endianness = Endianness.Big
            };
        }
    }
}