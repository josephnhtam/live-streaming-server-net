using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.Commands
{
    public class RtmpReceiveVideoCommandHandlerTest
    {
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly RtmpReceiveVideoCommandHandler _sut;

        public RtmpReceiveVideoCommandHandlerTest()
        {
            _clientContext = Substitute.For<IRtmpClientSessionContext>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _sut = new RtmpReceiveVideoCommandHandler();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task HandleAsync_Should_SetIsReceivingVideo_When_SubscriptionContextNotNull(bool flag)
        {
            // Arrange
            var command = new RtmpReceiveVideoCommand(0, new Dictionary<string, object>(), flag);

            var subscriptionContext = Substitute.For<IRtmpSubscribeStreamContext>();

            _clientContext.SubscribeStreamContext.Returns(subscriptionContext);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, CancellationToken.None);

            // Assert
            subscriptionContext.Received(1).IsReceivingVideo = flag;
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task HandleAsync_Should_StillReturnTrue_When_SubscriptionContextNull(bool flag)
        {
            // Arrange
            var command = new RtmpReceiveVideoCommand(0, new Dictionary<string, object>(), flag);

            _clientContext.SubscribeStreamContext.Returns((IRtmpSubscribeStreamContext?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }
    }
}
