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
    internal class JobHandler_Tests
    {
        private SchedulerState state;
        private IJobLauncher launcher;
        private IStreamJob job;
        private JobHandler handler;

        [SetUp]
        public void TestSetup()
        {
            state = new SchedulerState(Task.CompletedTask, CancellationToken.None);
            
            job = Substitute.For<IStreamJob>();

            launcher = Substitute.For<IJobLauncher>();

            handler = new JobHandler(launcher);
        }

        [Test]
        public void Should_remove_finished_wait_task_and_launch_a_send_job()
        {
            var waitJobTask = Task.FromResult(new WaitingJobResult(job));

            state.WaitingJobs.Add(Task.CompletedTask);
            state.WaitingJobs.Add(waitJobTask);
            state.WaitingJobs.Add(Task.CompletedTask);

            handler.HandleCompletedJob(waitJobTask, state);

            state.WaitingJobs.Should().HaveCount(2);
            state.WaitingJobs.Should().NotContain(waitJobTask);

            launcher.Received(1).LaunchSendJob(job, state);
        }

        [Test]
        public void Should_remove_finished_send_task_and_launch_a_wait_job()
        {
            var sendJobTask = Task.FromResult(new SendingJobResult(job));

            state.SendingJobs.Add(Task.CompletedTask);
            state.SendingJobs.Add(sendJobTask);
            state.SendingJobs.Add(Task.CompletedTask);

            handler.HandleCompletedJob(sendJobTask, state);

            state.SendingJobs.Should().HaveCount(2);
            state.SendingJobs.Should().NotContain(sendJobTask);

            launcher.Received(1).LaunchWaitJob(job, state);
        }

        [Test]
        public void Should_do_nothing_when_completed_task_is_neither_send_job_nor_wait_job()
        {
            handler.HandleCompletedJob(Task.CompletedTask, state);

            launcher.ReceivedCalls().Should().BeEmpty();
        }
    }
}