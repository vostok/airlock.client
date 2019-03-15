using System;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Sink.Buffers;
using Buffer = Vostok.Hercules.Client.Sink.Buffers.Buffer;

namespace Vostok.Hercules.Client.Tests.Sink.Buffers
{
    [TestFixture]
    internal class Buffer_Tests_Writing
    {
        [Test]
        public void Should_correctly_write_short_values()
        {
            TestWriting(writer => writer.Write(short.MaxValue));
        }

        [Test]
        public void Should_correctly_write_int_values()
        {
            TestWriting(writer => writer.Write(int.MaxValue / 2));
        }

        [Test]
        public void Should_correctly_write_long_values()
        {
            TestWriting(writer => writer.Write(long.MaxValue / 2));
        }

        [Test]
        public void Should_correctly_write_ushort_values()
        {
            TestWriting(writer => writer.Write(ushort.MaxValue));
        }

        [Test]
        public void Should_correctly_write_uint_values()
        {
            TestWriting(writer => writer.Write(uint.MaxValue / 2));
        }

        [Test]
        public void Should_correctly_write_ulong_values()
        {
            TestWriting(writer => writer.Write(ulong.MaxValue / 2));
        }

        [Test]
        public void Should_correctly_write_float_values()
        {
            TestWriting(writer => writer.Write(float.MaxValue));
        }

        [Test]
        public void Should_correctly_write_double_values()
        {
            TestWriting(writer => writer.Write(double.MaxValue));
        }

        [Test]
        public void Should_correctly_write_byte_values()
        {
            TestWriting(writer => writer.Write(byte.MaxValue));
        }

        [Test]
        public void Should_correctly_write_bool_values()
        {
            TestWriting(writer => writer.Write(true));
            TestWriting(writer => writer.Write(false));
        }

        [Test]
        public void Should_correctly_write_guid_values()
        {
            var value = Guid.NewGuid();

            TestWriting(writer => writer.Write(value));
        }

        [Test]
        public void Should_correctly_write_varlen_uint_values()
        {
            TestWriting(writer => writer.WriteVarlen(23U));
            TestWriting(writer => writer.WriteVarlen(23456U));
            TestWriting(writer => writer.WriteVarlen(23456789U));
            TestWriting(writer => writer.WriteVarlen(2345678901U));
        }

        [Test]
        public void Should_correctly_write_varlen_ulong_values()
        {
            TestWriting(writer => writer.WriteVarlen(23UL));
            TestWriting(writer => writer.WriteVarlen(23456UL));
            TestWriting(writer => writer.WriteVarlen(23456789UL));
            TestWriting(writer => writer.WriteVarlen(2345678901UL));
            TestWriting(writer => writer.WriteVarlen(2345678901234567UL));
        }

        [Test]
        public void Should_correctly_write_byte_arrays_with_length()
        {
            var buffer = Guid.NewGuid().ToByteArray();

            TestWriting(writer => writer.WriteWithLength(buffer));
        }

        [Test]
        public void Should_correctly_write_byte_arrays_without_length()
        {
            var buffer = Guid.NewGuid().ToByteArray();

            TestWriting(writer => writer.WriteWithoutLength(buffer));
        }

        [Test]
        public void Should_correctly_write_byte_array_slices_with_length()
        {
            var buffer = Guid.NewGuid().ToByteArray();

            TestWriting(writer => writer.WriteWithLength(buffer, 3, 8));
        }

        [Test]
        public void Should_correctly_write_byte_array_slices_without_length()
        {
            var buffer = Guid.NewGuid().ToByteArray();

            TestWriting(writer => writer.WriteWithoutLength(buffer, 3, 8));
        }

        [Test]
        public void Should_correctly_write_string_values_with_given_encoding_and_length()
        {
            var value = Guid.NewGuid().ToString();

            TestWriting(writer => writer.WriteWithLength(value, Encoding.UTF32));
        }

        [Test]
        public void Should_correctly_write_string_values_with_given_encoding()
        {
            var value = Guid.NewGuid().ToString();

            TestWriting(writer => writer.WriteWithoutLength(value, Encoding.UTF32));
        }

        [Test]
        public void Should_correctly_write_string_values_with_length()
        {
            var value = Guid.NewGuid().ToString();

            TestWriting(writer => writer.WriteWithLength(value));
        }

        [Test]
        public void Should_correctly_write_string_values()
        {
            var value = Guid.NewGuid().ToString();

            TestWriting(writer => writer.WriteWithoutLength(value));
        }

        private static void TestWriting(Action<IBinaryWriter> write)
        {
            var rawWriter = new BinaryBufferWriter(1) { Endianness = Endianness.Big };
            var buffer = new Buffer(1, int.MaxValue, new MemoryManager(long.MaxValue));

            write(rawWriter);
            write(buffer);

            buffer.FilledSegment.Should().Equal(rawWriter.FilledSegment);
            buffer.Position.Should().Be(rawWriter.Position);
            buffer.Capacity.Should().Be(rawWriter.Buffer.Length);
        }
    }
}