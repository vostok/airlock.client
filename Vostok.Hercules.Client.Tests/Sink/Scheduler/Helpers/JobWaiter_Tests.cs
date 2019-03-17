using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Scheduler;
using Vostok.Hercules.Client.Sink.Scheduler.Helpers;

namespace Vostok.Hercules.Client.Tests.Sink.Scheduler.Helpers
{
    [TestFixture]
    internal class JobWaiter_Tests
    {
        private JobWaiter waiter;
        private SchedulerState state;

        private TaskCompletionSource<bool> source1;
        private TaskCompletionSource<bool> source2;
        private TaskCompletionSource<bool> source3;
        private TaskCompletionSource<bool> source4;
        private CancellationTokenSource cancellation;

        [SetUp]
        public void TestSetup()
        {
            source1 = new TaskCompletionSource<bool>();
            source2 = new TaskCompletionSource<bool>();
            source3 = new TaskCompletionSource<bool>();
            source4 = new TaskCompletionSource<bool>();

            cancellation = new CancellationTokenSource();
            cancellation.Token.Register(() => source4.TrySetResult(true));

            state = new SchedulerState(source4.Task, cancellation.Token);

            waiter = new JobWaiter(100.Milliseconds(), 2);
        }

        [Test]
        public void Should_only_wait_on_sending_tasks_when_max_parallelism_is_reached()
        {
            state.SendingJobs.Add(source1.Task);
            state.SendingJobs.Add(source2.Task);
            state.WaitingJobs.Add(source3.Task);

            var task = waiter.WaitForNextCompletedJob(state);

            task.IsCompleted.Should().BeFalse();

            source3.TrySetResult(true);

            task.IsCompleted.Should().BeFalse();

            source2.TrySetResult(true);

            task.IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Should_wait_on_sending_tasks_when_max_parallelism_is_not_reached()
        {
            state.SendingJobs.Add(source1.Task);
            state.WaitingJobs.Add(source2.Task);

            var task = waiter.WaitForNextCompletedJob(state);

            task.IsCompleted.Should().BeFalse();

            source1.TrySetResult(true);

            task.IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Should_wait_on_waiting_tasks_when_max_parallelism_is_not_reached()
        {
            state.SendingJobs.Add(source1.Task);
            state.WaitingJobs.Add(source2.Task);

            var task = waiter.WaitForNextCompletedJob(state);

            task.IsCompleted.Should().BeFalse();

            source2.TrySetResult(true);

            task.IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Should_wait_on_waiting_tasks_when_there_are_no_sending_tasks()
        {
            state.WaitingJobs.Add(source1.Task);
            state.WaitingJobs.Add(source2.Task);

            var task = waiter.WaitForNextCompletedJob(state);

            task.IsCompleted.Should().BeFalse();

            source2.TrySetResult(true);

            task.IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Should_wait_for_a_configured_delay_if_there_are_no_tasks_at_all()
        {
            var task = waiter.WaitForNextCompletedJob(state);

            task.Wait(10.Seconds()).Should().BeTrue();
        }

        [Test]
        public void Should_not_complete_after_a_configured_delay_if_there_are_any_tasks_at_all()
        {
            state.SendingJobs.Add(source1.Task);
            state.WaitingJobs.Add(source2.Task);

            var task = waiter.WaitForNextCompletedJob(state);

            task.Wait(500.Milliseconds()).Should().BeFalse();
        }

        [Test]
        public void Should_wait_on_cancellation_task()
        {
            state.SendingJobs.Add(source1.Task);
            state.SendingJobs.Add(source2.Task);
            state.WaitingJobs.Add(source3.Task);

            var task = waiter.WaitForNextCompletedJob(state);

            task.IsCompleted.Should().BeFalse();

            cancellation.Cancel();

            task.IsCompleted.Should().BeTrue();
        }
    }
}