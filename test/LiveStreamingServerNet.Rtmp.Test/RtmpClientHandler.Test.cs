using FluentAssertions;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEvents;
using LiveStreamingServerNet.Rtmp.RateLimiting.Contracts;
using Mediator;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test
{
    public class RtmpClientHandlerTest : IDisposable
    {
        private readonly IMediator _mediator;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly ILogger<RtmpClientHandler> _logger;
        private readonly IClientHandle _clientHandle;
        private readonly IRtmpClientContext _clientContext;
        private readonly IRtmpClientContextFactory _clientContextFactory;
        private readonly IBandwidthLimiter _bandwidthLimiter;
        private readonly IBandwidthLimiterFactory _bandwidthLimiterFactory;
        private readonly INetworkStream _networkStream;
        private readonly RtmpClientHandler _sut;

        public RtmpClientHandlerTest()
        {
            _mediator = Substitute.For<IMediator>();
            _eventDispatcher = Substitute.For<IRtmpServerConnectionEventDispatcher>();
            _logger = Substitute.For<ILogger<RtmpClientHandler>>();

            _clientHandle = Substitute.For<IClientHandle>();

            _clientContext = Substitute.For<IRtmpClientContext>();
            _clientContextFactory = Substitute.For<IRtmpClientContextFactory>();
            _clientContextFactory.Create(Arg.Any<IClientHandle>()).Returns(_clientContext);

            _bandwidthLimiter = Substitute.For<IBandwidthLimiter>();
            _bandwidthLimiter.ConsumeBandwidth(Arg.Any<long>()).Returns(true);

            _bandwidthLimiterFactory = Substitute.For<IBandwidthLimiterFactory>();
            _bandwidthLimiterFactory.Create().Returns(_bandwidthLimiter);

            _networkStream = Substitute.For<INetworkStream>();

            _sut = new RtmpClientHandler(_mediator, _eventDispatcher, _clientContextFactory, _logger, _bandwidthLimiterFactory);
        }

        public void Dispose()
        {
            _networkStream.Dispose();
        }

        [Fact]
        public async Task HandleClientLoopAsync_Should_SendRtmpHandshakeC0Event_When_ClientStateIsHandshakeC0()
        {
            // Arrange
            _mediator.Send(Arg.Any<RtmpHandshakeC0Event>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _clientContext.State.Returns(RtmpClientState.HandshakeC0);
            await _sut.InitializeAsync(_clientHandle);

            // Act
            await _sut.HandleClientLoopAsync(_networkStream, default);

            // Assert
            _ = _mediator.Received().Send(Arg.Any<RtmpHandshakeC0Event>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleClientLoopAsync_Should_SendRtmpHandshakeC1Event_When_ClientStateIsHandshakeC1()
        {
            // Arrange
            _mediator.Send(Arg.Any<RtmpHandshakeC1Event>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _clientContext.State.Returns(RtmpClientState.HandshakeC1);
            await _sut.InitializeAsync(_clientHandle);

            // Act
            await _sut.HandleClientLoopAsync(_networkStream, default);

            // Assert
            _ = _mediator.Received().Send(Arg.Any<RtmpHandshakeC1Event>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleClientLoopAsync_Should_SendRtmpHandshakeC2Event_When_ClientStateIsHandshakeC2()
        {
            // Arrange
            _mediator.Send(Arg.Any<RtmpHandshakeC1Event>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _clientContext.State.Returns(RtmpClientState.HandshakeC2);
            await _sut.InitializeAsync(_clientHandle);

            // Act
            await _sut.HandleClientLoopAsync(_networkStream, default);

            // Assert
            _ = _mediator.Received().Send(Arg.Any<RtmpHandshakeC2Event>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleClientLoopAsync_Should_SendRtmpChunkEvent_When_ClientStateIsHandshakeDone()
        {
            // Arrange
            _mediator.Send(Arg.Any<RtmpHandshakeC1Event>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _clientContext.State.Returns(RtmpClientState.HandshakeDone);
            await _sut.InitializeAsync(_clientHandle);

            // Act
            await _sut.HandleClientLoopAsync(_networkStream, default);

            // Assert
            _ = _mediator.Received().Send(Arg.Any<RtmpChunkEvent>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(RtmpClientState.HandshakeC0)]
        [InlineData(RtmpClientState.HandshakeC1)]
        [InlineData(RtmpClientState.HandshakeC2)]
        [InlineData(RtmpClientState.HandshakeDone)]
        public async Task HandleClientLoopAsync_Should_RetrunTrue_When_EventHandlingIsSuccessful(RtmpClientState state)
        {
            // Arrange
            _mediator.Send(Arg.Any<IRequest<RtmpEventConsumingResult>>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _clientContext.State.Returns(state);
            await _sut.InitializeAsync(_clientHandle);

            // Act
            var result = await _sut.HandleClientLoopAsync(_networkStream, default);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(RtmpClientState.HandshakeC0)]
        [InlineData(RtmpClientState.HandshakeC1)]
        [InlineData(RtmpClientState.HandshakeC2)]
        [InlineData(RtmpClientState.HandshakeDone)]
        public async Task HandleClientLoopAsync_Should_RetrunFalse_When_EventHandlingIsNotSuccessful(RtmpClientState state)
        {
            // Arrange
            _mediator.Send(Arg.Any<IRequest<RtmpEventConsumingResult>>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(false, 0));

            _clientContext.State.Returns(state);
            await _sut.InitializeAsync(_clientHandle);

            // Act
            var result = await _sut.HandleClientLoopAsync(_networkStream, default);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(RtmpClientState.HandshakeC0)]
        [InlineData(RtmpClientState.HandshakeC1)]
        [InlineData(RtmpClientState.HandshakeC2)]
        [InlineData(RtmpClientState.HandshakeDone)]
        public async Task HandleClientLoopAsync_Should_RetrunFalse_When_BandwidthLimitIsExceeded(RtmpClientState state)
        {
            // Arrange
            _mediator.Send(Arg.Any<IRequest<RtmpEventConsumingResult>>(), Arg.Any<CancellationToken>())
                .Returns(new RtmpEventConsumingResult(true, 0));

            _bandwidthLimiter.ConsumeBandwidth(Arg.Any<long>()).Returns(false);

            _clientContext.State.Returns(state);
            await _sut.InitializeAsync(_clientHandle);

            // Act
            var result = await _sut.HandleClientLoopAsync(_networkStream, default);

            // Assert
            result.Should().BeFalse();
        }
    }
}
