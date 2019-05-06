using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Tests.Functional.Helpers;

namespace Vostok.Hercules.Client.Tests.Functional
{
    internal abstract class HerculesSender_FunctionalTests
    {
        protected static readonly TimeSpan Timeout = 20.Seconds();
        protected readonly DateTimeOffset Timestamp = DateTimeOffset.UtcNow;

        protected Action<string, Action<IHerculesEventBuilder>> PushEvent;
        protected Helpers.Hercules Hercules;

        [SetUp]
        public void Setup()
        {
            if (Hercules == null)
                Hercules = new Helpers.Hercules();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Hercules?.Dispose();
        }

        [Test]
        public void Should_not_fail_on_duplicate_keys()
        {
            Write_and_read_single_event(
                x => x
                    .SetTimestamp(Timestamp)
                    .AddValue("key", 1)
                    .AddValue("key", 2));
        }

        [Test]
        public void Should_work_with_different_streams()
        {
            for (var i = 0; i < 3; ++i)
            {
                var number = i;

                Write_and_read_single_event(
                    x => x
                        .SetTimestamp(Timestamp)
                        .AddValue("x", number));
            }
        }

        [Test]
        public void Should_read_and_write_hercules_event_with_all_data_types()
        {
            var guid = Guid.NewGuid();
            var @bool = true;
            var @byte = (byte)42;
            var @double = Math.PI;
            var @float = (float)@double;
            var @int = int.MaxValue;
            var @long = long.MinValue;
            var @short = short.MinValue;
            var @string = "dotnet";

            var guidVec = new[] {Guid.NewGuid(), Guid.NewGuid()};
            var boolVec = new[] {true, false};
            var byteVec = new[] {(byte)42, (byte)25};
            var doubleVec = new[] {Math.PI, Math.E};
            var floatVec = doubleVec.Select(x => (float)x).ToArray();
            var intVec = new[] {1337, 31337, int.MaxValue, int.MinValue};
            var longVec = new[] {long.MaxValue, long.MinValue, (long)1e18 + 1};
            var shortVec = new short[] {1000, 2000};
            var stringVec = new[] {"dotnet", "hercules"};

            Write_and_read_single_event(
                x => x
                    .SetTimestamp(Timestamp)
                    .AddNull("null")
                    .AddValue("guid", guid)
                    .AddValue("bool", @bool)
                    .AddValue("byte", @byte)
                    .AddValue("double", @double)
                    .AddValue("float", @float)
                    .AddValue("int", @int)
                    .AddValue("long", @long)
                    .AddValue("short", @short)
                    .AddValue("string", @string)
                    .AddVector("guidVec", guidVec)
                    .AddVector("boolVec", boolVec)
                    .AddVector("byteVec", byteVec)
                    .AddVector("doubleVec", doubleVec)
                    .AddVector("floatVec", floatVec)
                    .AddVector("intVec", intVec)
                    .AddVector("longVec", longVec)
                    .AddVector("shortVec", shortVec)
                    .AddVector("stringVec", stringVec)
                    .AddVector("emptyVec", new int[0])
                    .AddContainer(
                        "container",
                        b => b
                            .AddValue("inner", "x")
                            .AddVector("innerVec", new[] {1, 2, 3}))
                    .AddVectorOfContainers(
                        "containerVec",
                        new Action<IHerculesTagsBuilder>[]
                        {
                            b => b
                                .AddValue("inner", "y")
                                .AddVector("innerVec", new long[] {1, 3, 5})
                        })
                    .AddVectorOfContainers("emptyContainerVec", new Action<IHerculesTagsBuilder>[0])
            );
        }

        private void Write_and_read_single_event(Action<IHerculesEventBuilder> eventBuilder)
        {
            using (Hercules.Management.CreateTemporaryStream(out var stream))
            {
                var expectedEvent = eventBuilder.ToEvent();

                PushEvent.ToStream(stream)(eventBuilder);
                Hercules.Stream.WaitForAnyRecord(stream);

                var readResult = Hercules.Stream.ReadEvents(stream, 1);

                readResult.Single().Should().Be(expectedEvent);
            }
        }
    }
}