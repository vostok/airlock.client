using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Daemon;
using Vostok.Hercules.Client.Sink.State;
using Vostok.Hercules.Client.Sink.Statistics;
using Vostok.Hercules.Client.Sink.Writing;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Hercules.Client.Tests
{
    internal class HerculesSink_Tests
    {
        private const string Stream = "logs";

        private IStreamStateFactory factory;
        private IStreamState state;
        private IBufferPool pool;
        private IBuffer buffer;
        private IRecordWriter writer;
        private IStatisticsCollector stats;
        private IDaemon daemon;
        private ILog log;

        private HerculesSink sink;

        [SetUp]
        public void TestSetup()
        {
            factory = Substitute.For<IStreamStateFactory>();
            factory.Create(Arg.Any<string>()).Returns(_ => state);

            state = Substitute.For<IStreamState>();
            state.BufferPool.Returns(_ => pool);
            state.Statistics.Returns(_ => stats);
            state.RecordWriter.Returns(_ => writer);
            state.SendSignal.Returns(new AsyncManualResetEvent(false));

            buffer = Substitute.For<IBuffer>();
            writer = Substitute.For<IRecordWriter>();
            stats = Substitute.For<IStatisticsCollector>();
            daemon = Substitute.For<IDaemon>();

            pool = Substitute.For<IBufferPool>();
            pool.TryAcquire(out _)
                .Returns(
                    info =>
                    {
                        info[0] = buffer;
                        return true;
                    });

            log = new SynchronousConsoleLog();

            sink = new HerculesSink(factory, daemon, log);
        }

        [Test]
        public void Should_not_put_when_disposed()
        {
            sink.Dispose();

            sink.Put(Stream, _ => {});

            factory.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_not_put_when_given_a_null_stream()
        {
            sink.Put(null, _ => {});

            factory.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_not_put_when_given_a_null_builder()
        {
            sink.Put(Stream, null);

            factory.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Dispose_should_dispose_the_daemon_only_once()
        {
            sink.Dispose();
            sink.Dispose();
            sink.Dispose();

            daemon.Received(1).Dispose();
        }

        [Test]
        public void Should_report_an_overflow_when_failed_to_acquire_a_buffer()
        {
            pool.TryAcquire(out _).Returns(false);

            sink.Put(Stream, _ => {});

            stats.Received(1).ReportOverflow();
        }

        [Test]
        public void Should_set_send_signal_when_overflow_occurs_while_some_data_is_in_buffers()
        {
            pool.TryAcquire(out _).Returns(false);

            stats.EstimateStoredSize().Returns(1L);

            sink.Put(Stream, _ => {});

            state.SendSignal.WaitAsync().IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Should_not_set_send_signal_when_overflow_occurs_while_no_data_is_in_buffers()
        {
            pool.TryAcquire(out _).Returns(false);

            stats.EstimateStoredSize().Returns(0L);

            sink.Put(Stream, _ => {});

            state.SendSignal.WaitAsync().IsCompleted.Should().BeFalse();
        }

        [Test]
        public void Should_write_a_record()
        {
            sink.Put(Stream, _ => {});

            writer.Received(1).TryWrite(buffer, Arg.Any<Action<IHerculesEventBuilder>>(), out _);
        }

        [Test]
        public void Should_release_the_buffer_back_to_pool()
        {
            sink.Put(Stream, _ => {});

            pool.Received(1).Release(buffer);
        }

        [Test]
        public void Should_initialize_sending_daemon()
        {
            sink.Put(Stream, _ => {});

            daemon.Received(1).Initialize();
        }

        [Test]
        public void Should_reuse_stream_states()
        {
            for (var i = 0; i < 10; i++)
            {
                sink.Put(Stream, _ => {});
            }

            writer.ReceivedCalls().Should().HaveCount(10);
            daemon.ReceivedCalls().Should().HaveCount(10);
            pool.ReceivedCalls().Should().HaveCount(20);
        }
    }
}