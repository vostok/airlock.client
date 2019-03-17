using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Job;
using Vostok.Hercules.Client.Sink.Scheduler;
using Vostok.Hercules.Client.Sink.Scheduler.Helpers;

namespace Vostok.Hercules.Client.Tests.Sink.Scheduler.Helpers
{
    [TestFixture]
    internal class JobLauncher_Tests
    {
        private JobLauncher launcher;
        private SchedulerState state;
        private IStreamJob job;

        [SetUp]
        public void TestSetup()
        {
            launcher = new JobLauncher();
            state = new SchedulerState(Task.CompletedTask, CancellationToken.None);
            job = Substitute.For<IStreamJob>();
        }

        [Test]
        public void LaunchWaitJob_should_launch_wait_task_on_provided_job()
        {
            launcher.LaunchWaitJob(job, state);

            job.Received().WaitForNextSendAsync(state.CancellationToken);
        }

        [Test]
        public void LaunchWaitJob_should_add_wait_task_to_the_state()
        {
            launcher.LaunchWaitJob(job, state);

            state.WaitingJobs.Should().ContainSingle().Which.Should().BeAssignableTo<Task<WaitingJobResult>>();
        }

        [Test]
        public void LaunchSendJob_should_launch_send_task_on_provided_job()
        {
            launcher.LaunchSendJob(job, state);

            job.Received().SendAsync(state.CancellationToken);
        }

        [Test]
        public void LaunchSendJob_should_add_send_task_to_the_state()
        {
            launcher.LaunchSendJob(job, state);

            state.SendingJobs.Should().ContainSingle().Which.Should().BeAssignableTo<Task<SendingJobResult>>();
        }
    }
}