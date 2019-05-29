using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Sink.Job;
using Vostok.Hercules.Client.Sink.Planning;
using Vostok.Hercules.Client.Sink.Sender;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Hercules.Client.Tests.Sink.Job
{
    [TestFixture]
    internal class StreamJob_Tests
    {
        private static readonly TimeSpan RequestTimeout = 30.Seconds();

        private IStreamSender sender;
        private IPlanner planner;
        private ILog log;

        private StreamJob job;

        [SetUp]
        public void TestSetup()
        {
            sender = Substitute.For<IStreamSender>();
            sender
                .SendAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(SendResult(HerculesStatus.Success));

            planner = Substitute.For<IPlanner>();
            planner
                .WaitForNextSendAsync(Arg.Any<StreamSendResult>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            log = new SynchronousConsoleLog();

            job = new StreamJob(sender, planner, log, RequestTimeout);
        }

        [Test]
        public void Should_be_healthy_initially()
        {
            job.IsHealthy.Should().BeTrue();
        }

        [Test]
        public void Should_remain_healthy_after_successful_sends()
        {
            Send();
            Send();

            job.IsHealthy.Should().BeTrue();
        }

        [Test]
        public void Should_become_unhealthy_after_failed_send()
        {
            sender
                .SendAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(SendResult(HerculesStatus.NetworkError));

            Send();

            job.IsHealthy.Should().BeFalse();
        }

        [Test]
        public void Should_become_healthy_after_successful_send()
        {
            sender
                .SendAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(SendResult(HerculesStatus.NetworkError), SendResult(HerculesStatus.Success));

            Send();

            job.IsHealthy.Should().BeFalse();

            Send();

            job.IsHealthy.Should().BeTrue();
        }

        [Test]
        public void Should_always_pass_last_send_result_to_planner()
        {
            var result1 = SendResult(HerculesStatus.Success);
            var result2 = SendResult(HerculesStatus.Success);

            sender
                .SendAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(result1, result2);

            Send();
            Wait();

            planner.Received(1).WaitForNextSendAsync(result1, CancellationToken.None);

            Send();
            Wait();

            planner.Received(1).WaitForNextSendAsync(result2, CancellationToken.None);
        }

        [Test]
        public void Should_handle_sender_exceptions()
        {
            sender
                .SendAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("I have failed."));

            Send();
        }

        private static StreamSendResult SendResult(HerculesStatus status)
            => new StreamSendResult(status, TimeSpan.Zero);

        private void Send()
            => job.SendAsync(CancellationToken.None).GetAwaiter().GetResult();

        private void Wait()
            => job.WaitForNextSendAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}