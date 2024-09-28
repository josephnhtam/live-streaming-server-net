using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.Commands
{
    public class RtmpReceiveAudioCommandHandlerTest
    {
        private readonly Fixture _fixture;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly RtmpReceiveAudioCommandHandler _sut;

        public RtmpReceiveAudioCommandHandlerTest()
        {
            _fixture = new Fixture();
            _clientContext = Substitute.For<IRtmpClientSessionContext>();
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

            var streamId = _fixture.Create<uint>();
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStreamContext(streamId).Returns(streamContext);
            streamContext.SubscribeContext.Returns(subscribeStreamContext);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, CancellationToken.None);

            // Assert
            subscribeStreamContext.Received(1).IsReceivingAudio = flag;
            result.Should().BeTrue();
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task HandleAsync_Should_StillReturnTrue_When_SubscribeContextIsNotCreated(bool flag)
        {
            // Arrange
            var command = new RtmpReceiveAudioCommand(0, new Dictionary<string, object>(), flag);

            var streamId = _fixture.Create<uint>();
            var streamContext = Substitute.For<IRtmpStreamContext>();

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStreamContext(streamId).Returns(streamContext);
            streamContext.SubscribeContext.Returns((IRtmpSubscribeStreamContext?)null);

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
            var command = new RtmpReceiveAudioCommand(0, new Dictionary<string, object>(), flag);

            var streamId = _fixture.Create<uint>();
            var streamContext = Substitute.For<IRtmpStreamContext>();

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStreamContext(Arg.Any<uint>()).Returns((IRtmpStreamContext?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }
    }
}
