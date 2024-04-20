using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Commands
{
    public class RtmpReceiveAudioCommandHandlerTest
    {
        private readonly IRtmpClientContext _clientContext;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly RtmpReceiveAudioCommandHandler _sut;

        public RtmpReceiveAudioCommandHandlerTest()
        {
            _clientContext = Substitute.For<IRtmpClientContext>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _sut = new RtmpReceiveAudioCommandHandler();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task HandleAsync_Should_SetIsReceivingAudio_When_SubscriptionContextNotNull(bool flag)
        {
            // Arrange
            var command = new RtmpReceiveAudioCommand(0, new Dictionary<string, object>(), flag);

            var subscriptionContext = Substitute.For<IRtmpStreamSubscriptionContext>();

            _clientContext.StreamSubscriptionContext.Returns(subscriptionContext);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, CancellationToken.None);

            // Assert
            subscriptionContext.Received(1).IsReceivingAudio = flag;
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task HandleAsync_Should_StillReturnTrue_When_SubscriptionContextNull(bool flag)
        {
            // Arrange
            var command = new RtmpReceiveAudioCommand(0, new Dictionary<string, object>(), flag);

            _clientContext.StreamSubscriptionContext.Returns((IRtmpStreamSubscriptionContext?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }
    }
}
