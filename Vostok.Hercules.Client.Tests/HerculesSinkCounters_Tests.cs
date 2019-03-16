using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Hercules.Client.Tests
{
    [TestFixture]
    internal class HerculesSinkCounters_Tests
    {
        [Test]
        public void TotalLostRecords_should_return_a_sum_of_all_lost_records()
        {
            var counters = new HerculesSinkCounters((5, 151), (3, 134), (1, 18), 2, 13, 1 );

            counters.TotalLostRecords.Should().Be(3 + 2 + 13 + 1);
        }

        [Test]
        public void Should_correctly_sum_two_counters()
        {
            var counters1 = new HerculesSinkCounters((5, 151), (3, 134), (1, 18), 2, 13, 1);
            var counters2 = new HerculesSinkCounters((8, 235), (1, 54), (10, 180), 5, 1, 40);

            var sum1 = counters1 + counters2;
            var sum2 = counters2 + counters1;

            sum2.Should().BeEquivalentTo(sum1);

            sum1.SentRecords.Should().Be((13L, 386L));
            sum1.RejectedRecords.Should().Be((4L, 188L));
            sum1.StoredRecords.Should().Be((11L, 198L));
            sum1.RecordsLostDueToBuildFailures.Should().Be(7L);
            sum1.RecordsLostDueToSizeLimit.Should().Be(14L);
            sum1.RecordsLostDueToOverflows.Should().Be(41L);
        }
    }
}