using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers
{
    public class RtmpChunkEventHandlerTest
    {
        private readonly IRtmpMessageDispatcher<IRtmpClientSessionContext> _dispatcher;
        private readonly IRtmpAcknowledgementHandlerService _acknowledgementHandler;
        private readonly IRtmpChunkMessageAggregatorService _chunkMessageAggregator;
        private readonly ILogger<RtmpChunkEventHandler> _logger;
        private readonly RtmpChunkEvent _event;
        private readonly Fixture _fixture;
        private readonly INetworkStreamReader _networkStreamReader;
        private readonly IRtmpClientSessionContext _clientSessionContext;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly RtmpChunkEventHandler _sut;

        public RtmpChunkEventHandlerTest()
        {
            _fixture = new Fixture();
            _networkStreamReader = Substitute.For<INetworkStreamReader>();
            _clientSessionContext = Substitute.For<IRtmpClientSessionContext>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _dispatcher = Substitute.For<IRtmpMessageDispatcher<IRtmpClientSessionContext>>();
            _acknowledgementHandler = Substitute.For<IRtmpAcknowledgementHandlerService>();
            _chunkMessageAggregator = Substitute.For<IRtmpChunkMessageAggregatorService>();
            _logger = Substitute.For<ILogger<RtmpChunkEventHandler>>();

            _event = new RtmpChunkEvent { ClientContext = _clientSessionContext, NetworkStream = _networkStreamReader };
            _clientSessionContext.GetChunkStreamContext(Arg.Any<uint>()).Returns(_chunkStreamContext);
            _dispatcher.DispatchAsync(_chunkStreamContext, _clientSessionContext, Arg.Any<CancellationToken>()).Returns(true);

            _sut = new RtmpChunkEventHandler(_dispatcher, _chunkMessageAggregator, _acknowledgementHandler, _logger);
        }

        [Fact]
        internal async Task Handle_Should_DispatchRtmpMessage()
        {
            // Arrange
            _chunkMessageAggregator.AggregateChunkMessagesAsync(_networkStreamReader, _clientSessionContext, Arg.Any<CancellationToken>())
                .Returns(new RtmpChunkMessageAggregationResult(true, _fixture.Create<int>(), _chunkStreamContext));

            // Act
            await _sut.Handle(_event, default);

            // Assert
            _ = _dispatcher.Received(1).DispatchAsync(_chunkStreamContext, _clientSessionContext, Arg.Any<CancellationToken>());
        }

        [Fact]
        internal async Task Handle_Should_ResetChunkStreamContext()
        {
            // Arrange
            _chunkMessageAggregator.AggregateChunkMessagesAsync(_networkStreamReader, _clientSessionContext, Arg.Any<CancellationToken>())
                .Returns(new RtmpChunkMessageAggregationResult(_fixture.Create<bool>(), _fixture.Create<int>(), _chunkStreamContext));

            // Act
            await _sut.Handle(_event, default);

            // Assert
            _chunkMessageAggregator.Received(1).ResetChunkStreamContext(_chunkStreamContext);
        }

        [Fact]
        internal async Task Handle_Should_InvokeAcknowledgmentHandler()
        {
            // Arrange
            var chunkMessageSize = _fixture.Create<int>();

            _chunkMessageAggregator.AggregateChunkMessagesAsync(_networkStreamReader, _clientSessionContext, Arg.Any<CancellationToken>())
                .Returns(new RtmpChunkMessageAggregationResult(true, chunkMessageSize, _chunkStreamContext));

            // Act
            await _sut.Handle(_event, default);

            // Assert
            _acknowledgementHandler.Received(1).Handle(_clientSessionContext, chunkMessageSize);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        internal async Task Handle_Should_ReturnSucceeded_When_RtmpMessageHandlingIsSuccessful(bool success)
        {
            _chunkMessageAggregator.AggregateChunkMessagesAsync(_networkStreamReader, _clientSessionContext, Arg.Any<CancellationToken>())
                .Returns(new RtmpChunkMessageAggregationResult(true, _fixture.Create<int>(), _chunkStreamContext));

            _dispatcher.DispatchAsync(_chunkStreamContext, _clientSessionContext, Arg.Any<CancellationToken>())
                .Returns(success);

            // Act
            var result = await _sut.Handle(_event, default);

            // Assert
            result.Succeeded.Should().Be(success);
        }
    }
}
