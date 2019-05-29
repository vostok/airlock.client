using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Sink.Scheduler;

namespace Vostok.Hercules.Client.Tests.Sink.Daemon
{
    [TestFixture]
    internal class Daemon_Tests
    {
        private IScheduler scheduler;
        private Hercules.Client.Sink.Daemon.Daemon daemon;

        private int schedulerRuns;
        private AsyncManualResetEvent initSignal;
        private AsyncManualResetEvent endSignal;

        [SetUp]
        public void TestSetup()
        {
            schedulerRuns = 0;
            initSignal = new AsyncManualResetEvent(false);
            endSignal = new AsyncManualResetEvent(false);

            scheduler = Substitute.For<IScheduler>();
            scheduler.RunAsync(Arg.Any<CancellationToken>())
                .Returns(
                    info => Task.Run(
                        async () =>
                        {
                            var cancellationToken = info.Arg<CancellationToken>();

                            Interlocked.Increment(ref schedulerRuns);

                            initSignal.Set();

                            var cancellationSignal = new AsyncManualResetEvent(false);

                            using (cancellationToken.Register(() => cancellationSignal.Set()))
                                await cancellationSignal;

                            endSignal.Set();

                            cancellationToken.ThrowIfCancellationRequested();
                        }));

            daemon = new Hercules.Client.Sink.Daemon.Daemon(scheduler);
        }

        [Test]
        public void Initialize_should_only_run_scheduler_task_once()
        {
            daemon.Initialize();
            daemon.Initialize();
            daemon.Initialize();

            initSignal.WaitAsync().Wait(10.Seconds()).Should().BeTrue();

            Action assertion = () => schedulerRuns.Should().Be(1);

            assertion();

            assertion.ShouldNotFailIn(100.Milliseconds(), 10.Milliseconds());
        }

        [Test]
        public void Initialize_should_not_run_scheduler_if_already_disposed()
        {
            daemon.Dispose();

            daemon.Initialize();

            initSignal.WaitAsync().Wait(200.Milliseconds()).Should().BeFalse();
        }

        [Test]
        public void Dispose_should_not_fail_if_daemon_has_not_been_initialized_yet()
        {
            daemon.Dispose();
        }

        [Test]
        public void Dispose_should_wait_for_scheduler_task_to_finish()
        {
            daemon.Initialize();

            initSignal.GetAwaiter().GetResult();

            daemon.Dispose();

            endSignal.WaitAsync().IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Dispose_should_tolerate_multiple_calls()
        {
            daemon.Initialize();

            initSignal.GetAwaiter().GetResult();

            daemon.Dispose();
            daemon.Dispose();
            daemon.Dispose();
        }
    }
}