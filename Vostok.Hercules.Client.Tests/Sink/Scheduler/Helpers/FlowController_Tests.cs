using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Scheduler;
using Vostok.Hercules.Client.Sink.Scheduler.Helpers;

// ReSharper disable NotAccessedField.Local

namespace Vostok.Hercules.Client.Tests.Sink.Scheduler.Helpers
{
    [TestFixture]
    internal class FlowController_Tests
    {
        private CancellationTokenSource cancellation;
        private SchedulerState state;
        private object ownerObject;
        private FlowController controller;

        [SetUp]
        public void TestSetup()
        {
            cancellation = new CancellationTokenSource();
            state = new SchedulerState(Task.CompletedTask, cancellation.Token);
            controller = new FlowController(new WeakReference(ownerObject = new object()));
        }

        [Test]
        public void ShouldStillOperateOn_should_return_true_when_not_canceled_and_owner_is_still_alive()
        {
            controller.ShouldStillOperateOn(state).Should().BeTrue();
        }

        [Test]
        public void ShouldStillOperateOn_should_return_false_when_canceled()
        {
            cancellation.Cancel();

            controller.ShouldStillOperateOn(state).Should().BeFalse();
        }

        [Test]
        public void ShouldStillOperateOn_should_return_false_when_owner_object_gets_collected_by_GC()
        {
            ownerObject = null;

            GC.Collect();

            controller.ShouldStillOperateOn(state).Should().BeFalse();
        }
    }
}