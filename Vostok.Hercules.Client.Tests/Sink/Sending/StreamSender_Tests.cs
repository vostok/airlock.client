using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.Sending;
using Vostok.Hercules.Client.Sink.Statistics;
using Vostok.Hercules.Client.Sink.StreamState;
using Vostok.Logging.Console;

namespace Vostok.Hercules.Client.Tests.Sink.Sending
{
    [TestFixture]
    internal class StreamSender_Tests
    {
        private const string GlobalApiKey = "global-key";
        private const string StreamName = "my-stream";
        private const int RecordsPerBuffer = 10;

        private static readonly TimeSpan Timeout = 1.Minutes();

        private IStatisticsCollector stats;
        private IBufferPool pool;
        private IBufferSnapshotBatcher batcher;
        private IStreamState state;
        private IRequestContentFactory contentFactory;
        private IGateRequestSender requestSender;
        private IGateResponseClassifier responseClassifier;
        private StreamSettings settings;

        private CancellationTokenSource cancellation;

        private StreamSender sender;

        [SetUp]
        public void TestSetup()
        {
            state = Substitute.For<IStreamState>();
            state.Name.Returns(StreamName);
            state.BufferPool.Returns(pool = Substitute.For<IBufferPool>());
            state.Statistics.Returns(stats = Substitute.For<IStatisticsCollector>());
            state.Settings.Returns(settings = new StreamSettings());

            batcher = new BufferSnapshotBatcher(1);
            contentFactory = new RequestContentFactory();
            responseClassifier = new GateResponseClassifier(new ResponseAnalyzer(ResponseAnalysisContext.Stream));

            requestSender = Substitute.For<IGateRequestSender>();

            sender = new StreamSender(() => GlobalApiKey, state, batcher, contentFactory, 
                requestSender, responseClassifier, new SynchronousConsoleLog());

            cancellation = new CancellationTokenSource();

            SetupBuffers(50, 100, 150);
            SetupResponses(ResponseCode.Ok);
        }

        [Test]
        public void Should_return_nothing_to_send_result_when_all_collected_snapshots_are_null()
        {
            SetupBuffers(null, null, null);

            Send().Should().Be(StreamSendResult.NothingToSend);

            requestSender.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_return_nothing_to_send_result_when_all_collected_snapshots_are_empty()
        {
            SetupBuffers(0, 0, 0);

            Send().Should().Be(StreamSendResult.NothingToSend);

            requestSender.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_respect_cancellation_token()
        {
            cancellation.Cancel();

            Action action = () => Send();

            action.Should().Throw<OperationCanceledException>();
        }

        [Test]
        public void Should_return_successful_result_when_all_batches_are_successfully_sent()
        {
            Send().Should().Be(StreamSendResult.Success);
        }

        [Test]
        public void Should_stop_on_first_transient_failure()
        {
            SetupResponses(ResponseCode.InternalServerError, ResponseCode.Ok);

            Send().Should().Be(StreamSendResult.Failure);

            requestSender.ReceivedCalls().Should().HaveCount(1);
        }

        [Test]
        public void Should_not_stop_on_first_definitive_failure()
        {
            SetupResponses(ResponseCode.NotFound, ResponseCode.NotFound, ResponseCode.Ok);

            Send().Should().Be(StreamSendResult.Failure);

            requestSender.ReceivedCalls().Should().HaveCount(3);
        }

        [Test]
        public void Should_use_api_key_from_stream_settings_if_any()
        {
            settings.ApiKeyProvider = () => "custom";

            Send();

            requestSender.Received(3).FireAndForgetAsync(Arg.Any<string>(), "custom", Arg.Any<CompositeContent>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_use_api_key_from_global_settings_if_not_overridden()
        {
            Send();

            requestSender.Received(3).FireAndForgetAsync(Arg.Any<string>(), GlobalApiKey, Arg.Any<CompositeContent>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_pass_correct_parameters_to_request_sender()
        {
            Send();

            requestSender.Received(3).FireAndForgetAsync(StreamName, Arg.Any<string>(), Arg.Any<CompositeContent>(), Timeout, cancellation.Token);
        }

        [Test]
        public void Should_perform_gc_on_success()
        {
            Send();

            foreach (var buffer in pool)
            {
                buffer.Received().ReportGarbage(Arg.Any<BufferState>());
            }
        }

        [Test]
        public void Should_perform_gc_on_definitive_failure()
        {
            SetupResponses(ResponseCode.NotFound);

            Send();

            foreach (var buffer in pool)
            {
                buffer.Received().ReportGarbage(Arg.Any<BufferState>());
            }
        }

        [Test]
        public void Should_not_perform_gc_on_transient_failure()
        {
            SetupResponses(ResponseCode.RequestTimeout);

            Send();

            foreach (var buffer in pool)
            {
                buffer.DidNotReceive().ReportGarbage(Arg.Any<BufferState>());
            }
        }

        [Test]
        public void Should_report_successful_sends()
        {
            SetupResponses(ResponseCode.NotFound);

            SetupBuffers(56, 132, 13);

            Send();

            stats.ReceivedCalls().Should().HaveCount(3);
            stats.Received(1).ReportSendingFailure(RecordsPerBuffer, 56);
            stats.Received(1).ReportSendingFailure(RecordsPerBuffer, 132);
            stats.Received(1).ReportSendingFailure(RecordsPerBuffer, 13);
        }

        [Test]
        public void Should_report_definitely_failed_sends()
        {
            SetupBuffers(56, 132, 13);

            Send();

            stats.ReceivedCalls().Should().HaveCount(3);
            stats.Received(1).ReportSuccessfulSending(RecordsPerBuffer, 56);
            stats.Received(1).ReportSuccessfulSending(RecordsPerBuffer, 132);
            stats.Received(1).ReportSuccessfulSending(RecordsPerBuffer, 13);
        }

        [Test]
        public void Should_not_report_transitively_failed_sends()
        {
            SetupResponses(ResponseCode.ServiceUnavailable);

            Send();

            stats.ReceivedCalls().Should().BeEmpty();
        }

        private void SetupBuffers(params int?[] snapshotSizes)
        {
            var buffers = new IBuffer[snapshotSizes.Length];

            for (var i = 0; i < snapshotSizes.Length; i++)
            {
                buffers[i] = Substitute.For<IBuffer>();

                var snapshotSize = snapshotSizes[i];
                if (snapshotSize == null)
                {
                    buffers[i].TryMakeSnapshot().ReturnsNull();
                }
                else
                {
                    var snapshotData = new byte[snapshotSize.Value];
                    var snapshotState = new BufferState(snapshotData.Length, RecordsPerBuffer);
                    var snapshot = new BufferSnapshot(buffers[i], snapshotState, snapshotData);

                    buffers[i].TryMakeSnapshot().Returns(snapshot);
                }
            }

            pool.GetEnumerator().Returns(_ => (buffers as IReadOnlyList<IBuffer>).GetEnumerator());
        }

        private void SetupResponses(params ResponseCode[] codes)
        {
            if (codes.Length == 0)
                return;

            requestSender.FireAndForgetAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<CompositeContent>(),
                    Arg.Any<TimeSpan>(),
                    Arg.Any<CancellationToken>())
                .Returns(new Response(codes.First()), codes.Skip(1).Select(code => new Response(code)).ToArray());
        }

        private StreamSendResult Send()
            => sender.SendAsync(Timeout, cancellation.Token).GetAwaiter().GetResult();
    }
}