using FluentAssertions;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Statistics;

namespace Vostok.Hercules.Client.Tests.Sink.Statistics
{
    [TestFixture]
    internal class StatisticsCollector_Tests
    {
        private StatisticsCollector collector;

        [SetUp]
        public void TestSetup()
        {
            collector = new StatisticsCollector();
        }

        [Test]
        public void Should_initially_return_zero_counters()
        {
            collector.GetCounters().Should().BeEquivalentTo(HerculesSinkCounters.Zero);
        }

        [Test]
        public void ReportStoredRecord_should_increase_stored_records_size_and_count()
        {
            collector.ReportStoredRecord(12);
            collector.GetCounters().StoredRecords.Should().Be((1L, 12L));

            collector.ReportStoredRecord(31);
            collector.GetCounters().StoredRecords.Should().Be((2L, 43L));

            collector.ReportStoredRecord(8);
            collector.GetCounters().StoredRecords.Should().Be((3L, 51L));
        }

        [Test]
        public void EstimateStoredSize_should_return_current_stored_size()
        {
            collector.ReportStoredRecord(12);
            collector.EstimateStoredSize().Should().Be(12L);

            collector.ReportStoredRecord(31);
            collector.EstimateStoredSize().Should().Be(43L);

            collector.ReportStoredRecord(8);
            collector.EstimateStoredSize().Should().Be(51L);
        }

        [Test]
        public void ReportSuccessfulSending_should_increase_sent_records_size_and_count()
        {
            collector.ReportStoredRecord(12);
            collector.ReportStoredRecord(31);
            collector.ReportStoredRecord(8);

            collector.ReportSuccessfulSending(2, 43);

            collector.GetCounters().SentRecords.Should().Be((2L, 43L));

            collector.ReportSuccessfulSending(1, 8);

            collector.GetCounters().SentRecords.Should().Be((3L, 51L));
        }

        [Test]
        public void ReportSuccessfulSending_should_reduce_stored_records_size_and_count()
        {
            collector.ReportStoredRecord(12);
            collector.ReportStoredRecord(31);
            collector.ReportStoredRecord(8);

            collector.ReportSuccessfulSending(2, 43);

            collector.GetCounters().StoredRecords.Should().Be((1L, 8L));

            collector.ReportSuccessfulSending(1, 8);

            collector.GetCounters().StoredRecords.Should().Be((0L, 0L));
        }

        [Test]
        public void ReportSendingFailure_should_increase_rejected_records_size_and_count()
        {
            collector.ReportStoredRecord(12);
            collector.ReportStoredRecord(31);
            collector.ReportStoredRecord(8);

            collector.ReportSendingFailure(2, 43);

            collector.GetCounters().RejectedRecords.Should().Be((2L, 43L));

            collector.ReportSendingFailure(1, 8);

            collector.GetCounters().RejectedRecords.Should().Be((3L, 51L));
        }

        [Test]
        public void ReportSendingFailure_should_increase_total_lost_events_count()
        {
            collector.ReportStoredRecord(12);
            collector.ReportStoredRecord(31);
            collector.ReportStoredRecord(8);

            collector.ReportSendingFailure(2, 43);

            collector.GetCounters().TotalLostRecords.Should().Be(2);

            collector.ReportSendingFailure(1, 8);

            collector.GetCounters().TotalLostRecords.Should().Be(3);
        }

        [Test]
        public void ReportSendingFailure_should_reduce_stored_records_size_and_count()
        {
            collector.ReportStoredRecord(12);
            collector.ReportStoredRecord(31);
            collector.ReportStoredRecord(8);

            collector.ReportSendingFailure(2, 43);

            collector.GetCounters().StoredRecords.Should().Be((1L, 8L));

            collector.ReportSendingFailure(1, 8);

            collector.GetCounters().StoredRecords.Should().Be((0L, 0L));
        }

        [Test]
        public void ReportOverflow_should_increase_overflows_count()
        {
            for (var i = 1; i <= 5; i++)
            {
                collector.ReportOverflow();

                collector.GetCounters().RecordsLostDueToOverflows.Should().Be(i);
            }
        }

        [Test]
        public void ReportOverflow_should_increase_total_lost_events_count()
        {
            for (var i = 1; i <= 5; i++)
            {
                collector.ReportOverflow();

                collector.GetCounters().TotalLostRecords.Should().Be(i);
            }
        }

        [Test]
        public void ReportSizeLimitViolation_should_increase_limit_violations_count()
        {
            for (var i = 1; i <= 5; i++)
            {
                collector.ReportSizeLimitViolation();

                collector.GetCounters().RecordsLostDueToSizeLimit.Should().Be(i);
            }
        }

        [Test]
        public void ReportSizeLimitViolation_should_increase_total_lost_events_count()
        {
            for (var i = 1; i <= 5; i++)
            {
                collector.ReportSizeLimitViolation();

                collector.GetCounters().TotalLostRecords.Should().Be(i);
            }
        }

        [Test]
        public void ReportRecordBuildFailure_should_increase_build_failures_count()
        {
            for (var i = 1; i <= 5; i++)
            {
                collector.ReportRecordBuildFailure();

                collector.GetCounters().RecordsLostDueToBuildFailures.Should().Be(i);
            }
        }

        [Test]
        public void ReportRecordBuildFailure_should_increase_total_lost_events_count()
        {
            for (var i = 1; i <= 5; i++)
            {
                collector.ReportRecordBuildFailure();

                collector.GetCounters().TotalLostRecords.Should().Be(i);
            }
        }

        [Test]
        public void ReportReservedSize_should_set_reserved_size()
        {
            for (var i = 1; i <= 5; i++)
            {
                collector.ReportReservedSize(i);

                collector.GetCounters().ReservedSize.Should().Be(i);
            }
        }
    }
}