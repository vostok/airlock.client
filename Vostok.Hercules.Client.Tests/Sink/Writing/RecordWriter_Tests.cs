using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Writing;
using Vostok.Logging.Console;
using Buffer = Vostok.Hercules.Client.Sink.Buffers.Buffer;

namespace Vostok.Hercules.Client.Tests.Sink.Writing
{
    [TestFixture]
    internal class RecordWriter_Tests
    {
        private Buffer buffer;
        private RecordWriter writer;
        private long originalPosition;
        private BufferState originalCommitted;

        [SetUp]
        public void TestSetup()
        {
            buffer = new Buffer(1, 200, new MemoryManager(1000));

            buffer.Write(Guid.NewGuid());
            buffer.CommitRecord(16);
            buffer.Write(Guid.NewGuid());
            buffer.CommitRecord(16);

            originalPosition = buffer.Position;
            originalCommitted = buffer.Committed;

            writer = new RecordWriter(new SynchronousConsoleLog(), () => DateTimeOffset.UtcNow, 1, 100);
        }

        [Test]
        public void Should_write_a_record_and_return_succesful_status_when_everything_goes_well()
        {
            Write(b => b.AddValue("key", 123)).Should().Be(RecordWriteResult.Success);

            buffer.Position.Should().BeGreaterThan(originalPosition);
        }

        [Test]
        public void Should_commit_written_records_in_case_of_success()
        {
            Write(b => b.AddValue("key", 123)).Should().Be(RecordWriteResult.Success);

            buffer.Committed.Length.Should().Be((int)buffer.Position);
            buffer.Committed.RecordsCount.Should().Be(originalCommitted.RecordsCount + 1);
        }

        [Test]
        public void Should_return_written_record_size_in_case_of_success()
        {
            Write(b => b.AddValue("key", 123), out var size).Should().Be(RecordWriteResult.Success);

            size.Should().Be((int)(buffer.Position - originalPosition));
        }

        [Test]
        public void Should_reset_overflowed_buffer()
        {
            buffer.IsOverflowed = true;

            Write(b => b.AddValue("key", 123)).Should().Be(RecordWriteResult.Success);

            buffer.IsOverflowed.Should().BeFalse();
            buffer.Position.Should().BeGreaterThan(originalPosition);
        }

        [Test]
        public void Should_roll_back_with_error_when_hitting_buffer_memory_limit()
        {
            Write(b => b.AddVector("bytes", new byte[250])).Should().Be(RecordWriteResult.OutOfMemory);

            buffer.IsOverflowed.Should().BeTrue();
            buffer.Position.Should().Be(originalPosition);
            buffer.Committed.Should().Be(originalCommitted);
        }

        [Test]
        public void Should_roll_back_with_error_when_build_delegate_throws_an_exception()
        {
            Write(_ => throw new Exception("I have failed.")).Should().Be(RecordWriteResult.Exception);

            buffer.Position.Should().Be(originalPosition);
            buffer.Committed.Should().Be(originalCommitted);
        }

        [Test]
        public void Should_roll_back_with_error_when_record_size_exceeds_allowed_limit()
        {
            Write(b => b.AddVector("bytes", new byte[125])).Should().Be(RecordWriteResult.RecordTooLarge);

            buffer.Position.Should().Be(originalPosition);
            buffer.Committed.Should().Be(originalCommitted);
        }

        private RecordWriteResult Write(Action<IHerculesEventBuilder> write)
            => Write(write, out _);

        private RecordWriteResult Write(Action<IHerculesEventBuilder> write, out int recordSize)
            => writer.TryWrite(buffer, write, out recordSize);
    }
}