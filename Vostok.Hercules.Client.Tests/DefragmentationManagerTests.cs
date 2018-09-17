using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Hercules.Client.Tests
{
    [TestFixture]
    public class DefragmentationManagerTests
    {
        [Test]
        public void Run_GarbageSegmentsAreEmpty_SourceArrayNotModified()
        {
            var sourceArray = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var source = new ArraySegment<byte>(sourceArray, 0, 10);
            var segments = Array.Empty<ILineSegment>();

            var position = DefragmentationManager.Run(source, segments);

            Assert.That(position, Is.EqualTo(10));
            sourceArray.Should().BeEquivalentTo(new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9});
        }

        [Test]
        public void Run_GarbageSegmentsAreAdjoinedOneAnother_SourceArrayModified()
        {
            var sourceArray = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var source = new ArraySegment<byte>(sourceArray, 0, 10);
            var segments = new[] {new LineSegment {Offset = 3, Length = 2}, new LineSegment {Offset = 5, Length = 3}};

            var position = DefragmentationManager.Run(source, segments);

            Assert.That(position, Is.EqualTo(5));
            sourceArray.Take(position).Should().BeEquivalentTo(new byte[] {0, 1, 2, 8, 9});
        }

        [Test]
        public void Run_GarbageSegmentsAreSeparatedOneAnother_SourceArrayModified()
        {
            var sourceArray = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var source = new ArraySegment<byte>(sourceArray, 0, 10);
            var segments = new[] {new LineSegment {Offset = 3, Length = 2}, new LineSegment {Offset = 6, Length = 3}};

            var position = DefragmentationManager.Run(source, segments);

            Assert.That(position, Is.EqualTo(5));
            sourceArray.Take(position).Should().BeEquivalentTo(new byte[] {0, 1, 2, 5, 9});
        }

        [Test]
        public void Run_GarbageSegmentsCoversSourceArray_SourceArrayIsEmpty()
        {
            var sourceArray = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var source = new ArraySegment<byte>(sourceArray, 0, 10);
            var segments = new[] {new LineSegment {Offset = 0, Length = 5}, new LineSegment {Offset = 5, Length = 5}};

            var position = DefragmentationManager.Run(source, segments);

            Assert.That(position, Is.EqualTo(0));
            sourceArray.Take(position).Should().BeEquivalentTo(Array.Empty<byte>());
        }

        private class LineSegment : ILineSegment
        {
            public int Offset { get; set; }
            public int Length { get; set; }
        }
    }
}