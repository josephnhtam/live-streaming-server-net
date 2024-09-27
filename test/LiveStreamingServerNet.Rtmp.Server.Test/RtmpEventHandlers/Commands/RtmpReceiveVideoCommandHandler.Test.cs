using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.Commands
{
    public class RtmpReceiveVideoCommandHandlerTest
    {
        private readonly Fixture _fixture;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly RtmpReceiveVideoCommandHandler _sut;

        public RtmpReceiveVideoCommandHandlerTest()
        {
            _fixture = new Fixture();
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

            var streamId = _fixture.Create<uint>();
            var stream = Substitute.For<IRtmpStream>();
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStream(streamId).Returns(stream);
            stream.SubscribeContext.Returns(subscribeStreamContext);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, CancellationToken.None);

            // Assert
            subscribeStreamContext.Received(1).IsReceivingVideo = flag;
            result.Should().BeTrue();
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task HandleAsync_Should_StillReturnTrue_When_SubscribeContextIsNotCreated(bool flag)
        {
            // Arrange
            var command = new RtmpReceiveVideoCommand(0, new Dictionary<string, object>(), flag);

            var streamId = _fixture.Create<uint>();
            var stream = Substitute.For<IRtmpStream>();

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStream(streamId).Returns(stream);
            stream.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task HandleAsync_Should_StillReturnTrue_When_StreamIsNotCreated(bool flag)
        {
            // Arrange
            var command = new RtmpReceiveVideoCommand(0, new Dictionary<string, object>(), flag);

            var streamId = _fixture.Create<uint>();
            var stream = Substitute.For<IRtmpStream>();

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStream(Arg.Any<uint>()).Returns((IRtmpStream?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }
    }
}
