using FluentAssertions;
using NUnit.Framework;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Sink.Analyzer;

namespace Vostok.Hercules.Client.Tests.Sink.Analyzer
{
    [TestFixture]
    internal class StatusAnalyzer_Tests
    {
        private StatusAnalyzer analyzer;

        [SetUp]
        public void TestSetup()
        {
            analyzer = new StatusAnalyzer();
        }

        [TestCase(HerculesStatus.Success, true)]
        [TestCase(HerculesStatus.RequestTooLarge, true)]
        [TestCase(HerculesStatus.IncorrectRequest, true)]
        [TestCase(HerculesStatus.InsufficientPermissions, false)]
        [TestCase(HerculesStatus.StreamNotFound, false)]
        [TestCase(HerculesStatus.NetworkError, false)]
        [TestCase(HerculesStatus.ServerError, false)]
        [TestCase(HerculesStatus.Canceled, false)]
        [TestCase(HerculesStatus.Throttled, false)]
        [TestCase(HerculesStatus.Timeout, false)]
        [TestCase(HerculesStatus.Unauthorized, false)]
        [TestCase(HerculesStatus.UnknownError, false)]
        public void ShouldDropStoredRecords_should_correctly_react_to_given_status(HerculesStatus status, bool expectedResult)
        {
            analyzer.ShouldDropStoredRecords(status).Should().Be(expectedResult);
        }

        [TestCase(HerculesStatus.Success, false)]
        [TestCase(HerculesStatus.RequestTooLarge, false)]
        [TestCase(HerculesStatus.IncorrectRequest, false)]
        [TestCase(HerculesStatus.Canceled, false)]
        [TestCase(HerculesStatus.InsufficientPermissions, true)]
        [TestCase(HerculesStatus.StreamNotFound, true)]
        [TestCase(HerculesStatus.NetworkError, true)]
        [TestCase(HerculesStatus.ServerError, true)]
        [TestCase(HerculesStatus.Throttled, true)]
        [TestCase(HerculesStatus.Timeout, true)]
        [TestCase(HerculesStatus.Unauthorized, true)]
        [TestCase(HerculesStatus.UnknownError, true)]
        public void ShouldIncreaseSendPeriod_should_correctly_react_to_given_status(HerculesStatus status, bool expectedResult)
        {
            analyzer.ShouldIncreaseSendPeriod(status).Should().Be(expectedResult);
        }
    }
}