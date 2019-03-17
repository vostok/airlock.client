using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Job;
using Vostok.Hercules.Client.Sink.Scheduler;
using Vostok.Hercules.Client.Sink.Scheduler.Helpers;
using Vostok.Hercules.Client.Sink.State;

namespace Vostok.Hercules.Client.Tests.Sink.Scheduler.Helpers
{
    [TestFixture]
    internal class StateSynchronizer_Tests
    {
        private IStreamState state1;
        private IStreamState state2;
        private IStreamState state3;

        private IStreamStatesProvider statesProvider;
        private IStreamJobFactory jobFactory;
        private IJobLauncher jobLauncher;

        private SchedulerState state;
        private StateSynchronizer synchronizer;

        [SetUp]
        public void TestSetup()
        {
            state1 = Substitute.For<IStreamState>();
            state2 = Substitute.For<IStreamState>();
            state3 = Substitute.For<IStreamState>();

            state1.Name.Returns("state1");
            state2.Name.Returns("state2");
            state3.Name.Returns("state3");

            statesProvider = Substitute.For<IStreamStatesProvider>();
            statesProvider.GetStates().Returns(new[] {state1, state2, state3});

            jobFactory = Substitute.For<IStreamJobFactory>();
            jobLauncher = Substitute.For<IJobLauncher>();

            state = new SchedulerState(Task.CompletedTask, CancellationToken.None);

            synchronizer = new StateSynchronizer(statesProvider, jobFactory, jobLauncher);
        }

        [Test]
        public void Should_add_and_launch_wait_jobs_for_missing_states()
        {
            state.AllJobs[state2.Name] = Substitute.For<IStreamJob>();

            synchronizer.Synchronize(state);

            state.AllJobs.Should().HaveCount(3);
            state.AllJobs.Keys.Should().BeEquivalentTo("state1", "state2", "state3");

            jobFactory.ReceivedCalls().Should().HaveCount(2);
            jobFactory.Received(1).CreateJob(state1);
            jobFactory.Received(1).CreateJob(state3);

            jobLauncher.ReceivedCalls().Should().HaveCount(2);
            jobLauncher.Received(1).LaunchWaitJob(state.AllJobs[state1.Name], state);
            jobLauncher.Received(1).LaunchWaitJob(state.AllJobs[state3.Name], state);
        }
    }
}