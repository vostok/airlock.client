using System;
using System.Text;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Sink.Buffers;
using Buffer = Vostok.Hercules.Client.Sink.Buffers.Buffer;

namespace Vostok.Hercules.Client.Tests.Sink.Buffers
{
    [TestFixture]
    internal class Buffer_Tests_Writing
    {
        private const int InitialSize = 16;
        private const int MaximumSize = 100;
        private IMemoryManager manager;
        private Buffer buffer;

        [SetUp]
        public void TestSetup()
        {
            manager = Substitute.For<IMemoryManager>();
            manager.TryReserveBytes(Arg.Any<long>()).Returns(true);

            buffer = new Buffer(InitialSize, MaximumSize, manager);
        }

        [Test]
        public void Should_initially_have_zero_position()
        {
            buffer.Position.Should().Be(0L);
        }

        [Test]
        public void Should_have_big_endianness()
        {
            buffer.Endianness.Should().Be(Endianness.Big);
        }

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
            var data = Guid.NewGuid().ToByteArray();

            TestWriting(writer => writer.WriteWithLength(data));
        }

        [Test]
        public void Should_correctly_write_byte_arrays_without_length()
        {
            var data = Guid.NewGuid().ToByteArray();

            TestWriting(writer => writer.WriteWithoutLength(data));
        }

        [Test]
        public void Should_correctly_write_byte_array_slices_with_length()
        {
            var data = Guid.NewGuid().ToByteArray();

            TestWriting(writer => writer.WriteWithLength(data, 3, 8));
        }

        [Test]
        public void Should_correctly_write_byte_array_slices_without_length()
        {
            var data = Guid.NewGuid().ToByteArray();

            TestWriting(writer => writer.WriteWithoutLength(data, 3, 8));
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

        [Test]
        public void Should_be_able_to_accomodate_fitting_strings_without_length_that_do_not_fit_by_maximum_encoding_consumption()
        {
            buffer.WriteWithoutLength(new string('-', MaximumSize));

            buffer.IsOverflowed.Should().BeFalse();
            buffer.Position.Should().Be(MaximumSize);
        }

        [Test]
        public void Should_be_able_to_accomodate_fitting_strings_with_length_that_do_not_fit_by_maximum_encoding_consumption()
        {
            buffer.WriteWithLength(new string('-', MaximumSize - sizeof(int)));

            buffer.IsOverflowed.Should().BeFalse();
            buffer.Position.Should().Be(MaximumSize);
        }

        [Test]
        public void Should_not_be_overflowed_by_default()
        {
            buffer.IsOverflowed.Should().BeFalse();
        }

        [Test]
        public void Should_become_overflowed_after_a_failed_write()
        {
            buffer.WriteWithoutLength(new byte[MaximumSize + 1]);

            buffer.IsOverflowed.Should().BeTrue();
        }

        [Test]
        public void Should_remain_overflowed_after_a_failed_write_even_if_next_write_still_fits()
        {
            buffer.WriteWithoutLength(new byte[MaximumSize + 1]);

            buffer.Write(Guid.NewGuid());

            buffer.IsOverflowed.Should().BeTrue();
            buffer.Position.Should().Be(0);
        }

        [Test]
        public void Should_not_expand_if_write_fits_exactly_into_current_capacity()
        {
            buffer.Write(Guid.NewGuid());

            buffer.Capacity.Should().Be(16);
            buffer.Position.Should().Be(16);
        }

        [Test]
        public void Should_be_able_to_expand_up_to_maximum_size()
        {
            for (var i = 0; i < 6; i++)
                buffer.Write(Guid.NewGuid());

            buffer.Write(int.MaxValue);
            buffer.Capacity.Should().Be(MaximumSize);
            buffer.Position.Should().Be(MaximumSize);
        }

        [Test]
        public void Should_grow_twice_as_large_when_expanding_if_it_does_not_violate_max_size_restriction()
        {
            buffer.Write(Guid.NewGuid());
            buffer.Write(int.MaxValue);

            buffer.Capacity.Should().Be(32);
            buffer.Position.Should().Be(20);
        }

        [Test]
        public void Should_grow_more_than_twice_as_large_when_incoming_data_is_large_enough()
        {
            buffer.WriteWithoutLength(new byte[51]);

            buffer.Capacity.Should().Be(51);
            buffer.Position.Should().Be(51);
        }

        [Test]
        public void Should_be_able_to_grow_twice_as_large_just_up_to_max_size()
        {
            buffer = new Buffer(MaximumSize / 2, MaximumSize, manager);

            for (var i = 0; i < 4; i++)
                buffer.Write(Guid.NewGuid());

            buffer.Capacity.Should().Be(MaximumSize);
            buffer.Position.Should().Be(64);
        }

        [Test]
        public void Should_reserve_current_capacity_in_memory_manager_when_growing_twice()
        {
            buffer.Write(Guid.NewGuid());
            buffer.Write(int.MaxValue);

            manager.ReceivedCalls().Should().HaveCount(1);
            manager.Received().TryReserveBytes(InitialSize);

            buffer.Write(Guid.NewGuid());

            manager.ReceivedCalls().Should().HaveCount(2);
            manager.Received().TryReserveBytes(InitialSize);
        }

        [Test]
        public void Should_reserve_current_capacity_in_memory_manager_when_growing_more_than_twice()
        {
            buffer.WriteWithoutLength(new byte[51]);

            manager.ReceivedCalls().Should().HaveCount(1);
            manager.Received().TryReserveBytes(51 - InitialSize);
        }

        [Test]
        public void Should_be_able_to_use_remaining_capacity_even_if_memory_manager_has_nothing_left()
        {
            manager.TryReserveBytes(default).ReturnsForAnyArgs(false);

            for (var i = 0; i < 4; i++)
            {
                buffer.Write(Guid.NewGuid().GetHashCode());
            }

            buffer.IsOverflowed.Should().BeFalse();

            buffer.Position.Should().Be(16);
        }

        [Test]
        public void Should_become_overflowed_when_memory_manager_rejects_ordinary_expansion()
        {
            manager.TryReserveBytes(default).ReturnsForAnyArgs(false);

            buffer.Write(Guid.NewGuid());

            buffer.Write(true);

            buffer.IsOverflowed.Should().BeTrue();
            buffer.Capacity.Should().Be(16);
        }

        [Test]
        public void Should_become_overflowed_when_memory_manager_rejects_expansion_to_input_size()
        {
            manager.TryReserveBytes(default).ReturnsForAnyArgs(false);

            buffer.Write(Guid.NewGuid());

            buffer.WriteWithoutLength(new byte[50]);

            buffer.IsOverflowed.Should().BeTrue();
            buffer.Capacity.Should().Be(16);
        }

        [Test]
        public void Should_become_overflowed_when_memory_manager_rejects_expansion_up_to_max_size()
        {
            manager.TryReserveBytes(default).ReturnsForAnyArgs(false);

            buffer.Write(Guid.NewGuid());

            buffer.WriteWithoutLength(new byte[MaximumSize - 16]);

            buffer.IsOverflowed.Should().BeTrue();
            buffer.Capacity.Should().Be(16);
        }

        private static void TestWriting(Action<IBinaryWriter> write)
        {
            var rawWriter = new BinaryBufferWriter(1) {Endianness = Endianness.Big};
            var buffer = new Buffer(1, int.MaxValue, new MemoryManager(long.MaxValue));

            write(rawWriter);
            write(buffer);

            buffer.CommitRecord((int)buffer.Position);

            buffer.CommittedSegment.Should().Equal(rawWriter.FilledSegment);
            buffer.Position.Should().Be(rawWriter.Position);
            buffer.Capacity.Should().Be(rawWriter.Buffer.Length);

            buffer = new Buffer(0, 0, new MemoryManager(0));

            write(buffer);

            buffer.IsOverflowed.Should().BeTrue();
        }
    }
}