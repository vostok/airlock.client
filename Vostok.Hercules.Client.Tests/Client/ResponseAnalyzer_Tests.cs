using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Client;

namespace Vostok.Hercules.Client.Tests.Client
{
    [TestFixture]
    internal class ResponseAnalyzer_Tests
    {
        private string errorMessage;

        [TestCase(ResponseCode.Ok, HerculesStatus.Success)]
        [TestCase(ResponseCode.BadRequest, HerculesStatus.IncorrectRequest)]
        [TestCase(ResponseCode.Unauthorized, HerculesStatus.Unauthorized)]
        [TestCase(ResponseCode.Forbidden, HerculesStatus.InsufficientPermissions)]
        [TestCase(ResponseCode.RequestEntityTooLarge, HerculesStatus.RequestTooLarge)]
        [TestCase(ResponseCode.TooManyRequests, HerculesStatus.Throttled)]
        [TestCase(ResponseCode.Canceled, HerculesStatus.Canceled)]
        [TestCase(ResponseCode.RequestTimeout, HerculesStatus.Timeout)]
        [TestCase(ResponseCode.ConnectFailure, HerculesStatus.NetworkError)]
        [TestCase(ResponseCode.SendFailure, HerculesStatus.NetworkError)]
        [TestCase(ResponseCode.ReceiveFailure, HerculesStatus.NetworkError)]
        [TestCase(ResponseCode.StreamReuseFailure, HerculesStatus.NetworkError)]
        [TestCase(ResponseCode.StreamInputFailure, HerculesStatus.NetworkError)]
        [TestCase(ResponseCode.InsufficientStorage, HerculesStatus.NetworkError)]
        [TestCase(ResponseCode.ProxyTimeout, HerculesStatus.NetworkError)]
        [TestCase(ResponseCode.BadGateway, HerculesStatus.NetworkError)]
        [TestCase(ResponseCode.InternalServerError, HerculesStatus.ServerError)]
        [TestCase(ResponseCode.ServiceUnavailable, HerculesStatus.ServerError)]
        [TestCase(ResponseCode.Unknown, HerculesStatus.UnknownError)]
        [TestCase(ResponseCode.UnknownFailure, HerculesStatus.UnknownError)]
        [TestCase(ResponseCode.UnsupportedMediaType, HerculesStatus.UnknownError)]
        [TestCase(ResponseCode.NotAcceptable, HerculesStatus.UnknownError)]
        [TestCase(ResponseCode.Gone, HerculesStatus.UnknownError)]
        [TestCase(ResponseCode.NotImplemented, HerculesStatus.UnknownError)]
        [TestCase(ResponseCode.MethodNotAllowed, HerculesStatus.UnknownError)]
        public void Should_correctly_map_given_common_response_code(ResponseCode code, HerculesStatus expectedStatus)
        {
            Analyze(code).Should().Be(expectedStatus);
        }

        [TestCase(ResponseCode.NotFound, HerculesStatus.StreamNotFound)]
        [TestCase(ResponseCode.Conflict, HerculesStatus.StreamAlreadyExists)]
        public void Should_correctly_map_given_stream_response_code(ResponseCode code, HerculesStatus expectedStatus)
        {
            Analyze(code).Should().Be(expectedStatus);
        }

        [TestCase(ResponseCode.NotFound, HerculesStatus.TimelineNotFound)]
        [TestCase(ResponseCode.Conflict, HerculesStatus.TimelineAlreadyExists)]
        public void Should_correctly_map_given_timeline_response_code(ResponseCode code, HerculesStatus expectedStatus)
        {
            Analyze(code, context: ResponseAnalysisContext.Timeline).Should().Be(expectedStatus);
        }

        [Test]
        public void Should_ignore_error_messages_for_successful_statuses()
        {
            Analyze(ResponseCode.Ok, "Ok!");

            errorMessage.Should().BeNull();
        }

        [Test]
        public void Should_ignore_long_error_messages()
        {
            Analyze(ResponseCode.Conflict, new string('-', 1000));

            errorMessage.Should().BeNull();
        }

        [Test]
        public void Should_extract_error_messages_for_failure_statuses()
        {
            Analyze(ResponseCode.Conflict, "Stream 's' already exists.");

            errorMessage.Should().Be("Stream 's' already exists.");
        }

        private HerculesStatus Analyze(ResponseCode code, string message = null, ResponseAnalysisContext context = ResponseAnalysisContext.Stream)
            => new ResponseAnalyzer(context).Analyze(CreateResponse(code, message), out errorMessage);

        private static Response CreateResponse(ResponseCode code, string message)
            => message == null ? new Response(code) : new Response(code).WithContent(message);
    }
}