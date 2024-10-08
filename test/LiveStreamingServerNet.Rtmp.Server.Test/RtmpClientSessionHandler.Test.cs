using FluentAssertions;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.Server.RateLimiting.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test
{
    public class RtmpClientSessionHandlerTest : IAsyncDisposable
    {
        private readonly IMediator _mediator;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly ILogger<RtmpClientSessionHandler> _logger;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IBandwidthLimiter _bandwidthLimiter;
        private readonly IBandwidthLimiterFactory _bandwidthLimiterFactory;
        private readonly INetworkStream _networkStream;
        private readonly RtmpClientSessionHandler _sut;

        public RtmpClientSessionHandlerTest()
        {
            _mediator = Substitute.For<IMediator>();
            _eventDispatcher = Substitute.For<IRtmpServerConnectionEventDispatcher>();
            _logger = Substitute.For<ILogger<RtmpClientSessionHandler>>();

            _clientContext = Substitute.For<IRtmpClientSessionContext>();

            _bandwidthLimiter = Substitute.For<IBandwidthLimiter>();
            _bandwidthLimiter.ConsumeBandwidth(Arg.Any<long>()).Returns(true);

            _bandwidthLimiterFactory = Substitute.For<IBandwidthLimiterFactory>();
            _bandwidthLimiterFactory.Create().Returns(_bandwidthLimiter);

            _networkStream = Substitute.For<INetworkStream>();

            _sut = new RtmpClientSessionHandler(_clientContext, _mediator, _eventDispatcher, _logger, _bandwidthLimiterFactory);
        }

        public async ValueTask DisposeAsync()
        {
            await _networkStream.DisposeAsync();
        }

        [Fact]
        internal async Task HandleClientLoopAsync_Should_SendRtmpHandshakeC0Event_When_ClientStateIsHandshakeC0()
        {
            // Arrange
            _mediator.Send(Arg.Any<RtmpHandshakeC0Event>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _clientContext.State.Returns(RtmpClientSessionState.HandshakeC0);
            await _sut.InitializeAsync(default);

            // Act
            await _sut.HandleSessionLoopAsync(_networkStream, default);

            // Assert
            _ = _mediator.Received().Send(Arg.Any<RtmpHandshakeC0Event>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        internal async Task HandleClientLoopAsync_Should_SendRtmpHandshakeC1Event_When_ClientStateIsHandshakeC1()
        {
            // Arrange
            _mediator.Send(Arg.Any<RtmpHandshakeC1Event>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _clientContext.State.Returns(RtmpClientSessionState.HandshakeC1);
            await _sut.InitializeAsync(default);

            // Act
            await _sut.HandleSessionLoopAsync(_networkStream, default);

            // Assert
            _ = _mediator.Received().Send(Arg.Any<RtmpHandshakeC1Event>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        internal async Task HandleClientLoopAsync_Should_SendRtmpHandshakeC2Event_When_ClientStateIsHandshakeC2()
        {
            // Arrange
            _mediator.Send(Arg.Any<RtmpHandshakeC1Event>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _clientContext.State.Returns(RtmpClientSessionState.HandshakeC2);
            await _sut.InitializeAsync(default);

            // Act
            await _sut.HandleSessionLoopAsync(_networkStream, default);

            // Assert
            _ = _mediator.Received().Send(Arg.Any<RtmpHandshakeC2Event>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        internal async Task HandleClientLoopAsync_Should_SendRtmpChunkEvent_When_ClientStateIsHandshakeDone()
        {
            // Arrange
            _mediator.Send(Arg.Any<RtmpHandshakeC1Event>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _clientContext.State.Returns(RtmpClientSessionState.HandshakeDone);
            await _sut.InitializeAsync(default);

            // Act
            await _sut.HandleSessionLoopAsync(_networkStream, default);

            // Assert
            _ = _mediator.Received().Send(Arg.Any<RtmpChunkEvent>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(RtmpClientSessionState.HandshakeC0)]
        [InlineData(RtmpClientSessionState.HandshakeC1)]
        [InlineData(RtmpClientSessionState.HandshakeC2)]
        [InlineData(RtmpClientSessionState.HandshakeDone)]
        internal async Task HandleClientLoopAsync_Should_RetrunTrue_When_EventHandlingIsSuccessful(RtmpClientSessionState state)
        {
            // Arrange
            _mediator.Send(Arg.Any<IRequest<RtmpEventConsumingResult>>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _clientContext.State.Returns(state);
            await _sut.InitializeAsync(default);

            // Act
            var result = await _sut.HandleSessionLoopAsync(_networkStream, default);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(RtmpClientSessionState.HandshakeC0)]
        [InlineData(RtmpClientSessionState.HandshakeC1)]
        [InlineData(RtmpClientSessionState.HandshakeC2)]
        [InlineData(RtmpClientSessionState.HandshakeDone)]
        internal async Task HandleClientLoopAsync_Should_RetrunFalse_When_EventHandlingIsNotSuccessful(RtmpClientSessionState state)
        {
            // Arrange
            _mediator.Send(Arg.Any<IRequest<RtmpEventConsumingResult>>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(false, 0));

            _clientContext.State.Returns(state);
            await _sut.InitializeAsync(default);

            // Act
            var result = await _sut.HandleSessionLoopAsync(_networkStream, default);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(RtmpClientSessionState.HandshakeC0)]
        [InlineData(RtmpClientSessionState.HandshakeC1)]
        [InlineData(RtmpClientSessionState.HandshakeC2)]
        [InlineData(RtmpClientSessionState.HandshakeDone)]
        internal async Task HandleClientLoopAsync_Should_RetrunFalse_When_BandwidthLimitIsExceeded(RtmpClientSessionState state)
        {
            // Arrange
            _mediator.Send(Arg.Any<IRequest<RtmpEventConsumingResult>>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _bandwidthLimiter.ConsumeBandwidth(Arg.Any<long>()).Returns(false);

            _clientContext.State.Returns(state);
            await _sut.InitializeAsync(default);

            // Act
            var result = await _sut.HandleSessionLoopAsync(_networkStream, default);

            // Assert
            result.Should().BeFalse();
        }
    }
}
