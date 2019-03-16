using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Statistics;
using Vostok.Hercules.Client.Sink.Writing;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Hercules.Client.Tests.Sink.Writing
{
    [TestFixture]
    internal class ReportingWriter_Tests
    {
        private IRecordWriter baseWriter;
        private IStatisticsCollector statistics;
        private ReportingWriter reportingWriter;

        [SetUp]
        public void TestSetup()
        {
            baseWriter = Substitute.For<IRecordWriter>();
            statistics = Substitute.For<IStatisticsCollector>();
            reportingWriter = new ReportingWriter(baseWriter, statistics);
        }

        [TestCase(RecordWriteResult.Success)]
        [TestCase(RecordWriteResult.Exception)]
        [TestCase(RecordWriteResult.OutOfMemory)]
        [TestCase(RecordWriteResult.RecordTooLarge)]
        public void Should_delegate_to_base_writer(RecordWriteResult result)
        {
            SetupResult(result, 35);

            Write(out var size).Should().Be(result);

            size.Should().Be(35);
        }

        [Test]
        public void Should_report_stored_records()
        {
            SetupResult(RecordWriteResult.Success, 120);

            Write(out _);

            statistics.Received().ReportStoredRecord(120);
        }

        [Test]
        public void Should_report_overflows()
        {
            SetupResult(RecordWriteResult.OutOfMemory, 120);

            Write(out _);

            statistics.Received().ReportOverflow();
        }

        [Test]
        public void Should_report_large_records()
        {
            SetupResult(RecordWriteResult.RecordTooLarge, 120);

            Write(out _);

            statistics.Received().ReportSizeLimitViolation();
        }

        [Test]
        public void Should_report_build_failures()
        {
            SetupResult(RecordWriteResult.Exception, 120);

            Write(out _);

            statistics.Received().ReportRecordBuildFailure();
        }

        private RecordWriteResult Write(out int recordSize)
            => reportingWriter.TryWrite(null, null, out recordSize);

        private void SetupResult(RecordWriteResult result, int recordSize)
            => baseWriter
                .TryWrite(null, null, out _)
                .ReturnsForAnyArgs(
                    info =>
                    {
                        info[2] = recordSize;
                        return result;
                    });
    }
}