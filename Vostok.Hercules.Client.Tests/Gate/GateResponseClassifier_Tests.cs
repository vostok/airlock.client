using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Gate;

namespace Vostok.Hercules.Client.Tests.Gate
{
    [TestFixture]
    internal class GateResponseClassifier_Tests
    {
        private IResponseAnalyzer responseAnalyzer;
        private GateResponseClassifier responseClassifier;

        [SetUp]
        public void TestSetup()
        {
            responseAnalyzer = Substitute.For<IResponseAnalyzer>();
            responseClassifier = new GateResponseClassifier(responseAnalyzer);
        }

        [TestCase(HerculesStatus.Success, GateResponseClass.Success)]

        [TestCase(HerculesStatus.NetworkError, GateResponseClass.TransientFailure)]
        [TestCase(HerculesStatus.ServerError, GateResponseClass.TransientFailure)]
        [TestCase(HerculesStatus.Throttled, GateResponseClass.TransientFailure)]
        [TestCase(HerculesStatus.Canceled, GateResponseClass.TransientFailure)]
        [TestCase(HerculesStatus.Timeout, GateResponseClass.TransientFailure)]

        [TestCase(HerculesStatus.Unauthorized, GateResponseClass.DefinitiveFailure)]
        [TestCase(HerculesStatus.IncorrectRequest, GateResponseClass.DefinitiveFailure)]
        [TestCase(HerculesStatus.InsufficientPermissions, GateResponseClass.DefinitiveFailure)]
        [TestCase(HerculesStatus.StreamNotFound, GateResponseClass.DefinitiveFailure)]
        [TestCase(HerculesStatus.RequestTooLarge, GateResponseClass.DefinitiveFailure)]
        public void Should_map_given_status_to_correct_response_class(HerculesStatus status, GateResponseClass expected)
        {
            responseAnalyzer.Analyze(Arg.Any<Response>(), out _).Returns(status);

            responseClassifier.Classify(Responses.Unknown).Should().Be(expected);
        }
    }
}