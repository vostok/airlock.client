using System;
using System.Text;
using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client.Binary
{
    internal class HerculesBinaryWriter : IHerculesBinaryWriter
    {
        private readonly BinaryBufferWriter writer;

        public HerculesBinaryWriter(int initialCapacity)
        {
            this.writer = new BinaryBufferWriter(initialCapacity){Endianness = Endianness.Big};
        }

        public HerculesBinaryWriter(byte[] array)
        {
            this.writer = new BinaryBufferWriter(array){Endianness = Endianness.Big};
        }

        public int Position { get => (int) writer.Position; set => writer.Position = value; }
        public bool IsOverflowed { get; set; }
        public byte[] Array => writer.Buffer;
        public ArraySegment<byte> FilledSegment => writer.FilledSegment;
        public Encoding Encoding => Encoding.UTF8;
        public void Write(int value) => writer.Write(value);
        public void Write(long value) => writer.Write(value);
        public void Write(short value) => writer.Write(value);
        public void Write(double value) => writer.Write(value);
        public void Write(float value) => writer.Write(value);
        public void Write(byte value) => writer.Write(value);
        public void Write(bool value) => writer.Write(value);
        public void Write(ushort value) => writer.Write(value);
        public void Write(Guid value) => writer.Write(value);
        public void WriteWithoutLength(string value) => writer.WriteWithoutLength(value);
        public void WriteWithLength(string value) => writer.WriteWithLength(value);
        public void WriteWithLength(byte[] value, int offset, int length) => writer.WriteWithLength(value, offset, length);
        public void WriteWithoutLength(byte[] value, int offset, int length) => writer.WriteWithoutLength(value, offset, length);
    }
}