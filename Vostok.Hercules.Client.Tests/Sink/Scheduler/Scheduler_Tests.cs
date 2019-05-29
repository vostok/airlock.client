using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Job;
using Vostok.Hercules.Client.Sink.Scheduler;
using Vostok.Hercules.Client.Sink.Scheduler.Helpers;

namespace Vostok.Hercules.Client.Tests.Sink.Scheduler
{
    [TestFixture]
    internal class Scheduler_Tests
    {
        private IStateSynchronizer synchronizer;
        private IFlowController controller;
        private IJobWaiter jobWaiter;
        private IJobHandler jobHandler;

        private Task completedTask1;
        private Task completedTask2;

        private Hercules.Client.Sink.Scheduler.Scheduler scheduler;

        [SetUp]
        public void TestSetup()
        {
            synchronizer = Substitute.For<IStateSynchronizer>();

            controller = Substitute.For<IFlowController>();
            controller.ShouldStillOperateOn(Arg.Any<SchedulerState>()).Returns(true, true, true, true, false); // 2 iterations

            completedTask1 = Task.FromResult(true);
            completedTask2 = Task.FromResult(true);

            jobWaiter = Substitute.For<IJobWaiter>();
            jobWaiter
                .WaitForNextCompletedJob(Arg.Any<SchedulerState>())
                .Returns(completedTask1, completedTask2);

            jobHandler = Substitute.For<IJobHandler>();

            scheduler = new Hercules.Client.Sink.Scheduler.Scheduler(synchronizer, controller, jobWaiter, jobHandler);
        }

        [Test]
        public void Should_perform_iterations_until_controller_says_to_stop()
        {
            Run();

            controller.Received(5).ShouldStillOperateOn(Arg.Any<SchedulerState>());

            synchronizer.Received(3).Synchronize(Arg.Any<SchedulerState>());

            jobWaiter.Received(2).WaitForNextCompletedJob(Arg.Any<SchedulerState>());

            jobHandler.ReceivedCalls().Should().HaveCount(2);
            jobHandler.Received().HandleCompletedJob(completedTask1, Arg.Any<SchedulerState>());
            jobHandler.Received().HandleCompletedJob(completedTask2, Arg.Any<SchedulerState>());
        }

        [Test]
        public void Should_wait_for_all_sending_jobs_before_returning()
        {
            var sendingTaskSource = new TaskCompletionSource<bool>();

            synchronizer
                .When(s => s.Synchronize(Arg.Any<SchedulerState>()))
                .Do(info => info.Arg<SchedulerState>().SendingJobs.Add(sendingTaskSource.Task));

            var runTask = scheduler.RunAsync(CancellationToken.None);

            runTask.Wait(50).Should().BeFalse();

            sendingTaskSource.TrySetResult(true);

            runTask.Wait(10.Seconds()).Should().BeTrue();
        }

        [Test]
        public void Should_send_with_all_healthy_jobs_before_returning()
        {
            var job1 = Substitute.For<IStreamJob>();
            var job2 = Substitute.For<IStreamJob>();
            var job3 = Substitute.For<IStreamJob>();

            job1.IsHealthy.Returns(true);
            job2.IsHealthy.Returns(true);
            job3.IsHealthy.Returns(false);

            synchronizer
                .When(s => s.Synchronize(Arg.Any<SchedulerState>()))
                .Do(
                    info =>
                    {
                        var state = info.Arg<SchedulerState>();

                        state.AllJobs["job1"] = job1;
                        state.AllJobs["job2"] = job2;
                        state.AllJobs["job3"] = job3;
                    });

            Run();

            job1.Received().SendAsync(Arg.Any<CancellationToken>());
            job2.Received().SendAsync(Arg.Any<CancellationToken>());
            job3.DidNotReceive().SendAsync(Arg.Any<CancellationToken>());
        }

        private void Run()
            => scheduler.RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}