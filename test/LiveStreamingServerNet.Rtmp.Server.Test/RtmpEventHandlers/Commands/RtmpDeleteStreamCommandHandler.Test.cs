using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.Commands
{
    public class RtmpDeleteStreamCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpStreamDeletionService _streamDeletionService;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly RtmpDeleteStreamCommandHandler _sut;

        public RtmpDeleteStreamCommandHandlerTest()
        {
            _fixture = new Fixture();
            _streamDeletionService = Substitute.For<IRtmpStreamDeletionService>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientSessionContext>();

            _sut = new RtmpDeleteStreamCommandHandler(_streamDeletionService);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTrueAndDeleteStream_When_StreamExists()
        {
            // Arrange
            var transactionId = _fixture.Create<double>();
            var streamId = _fixture.Create<uint>();
            var stream = Substitute.For<IRtmpStream>();
            var commandObject = new Dictionary<string, object>();
            var command = new RtmpDeleteStreamCommand(transactionId, commandObject, streamId);

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStream(streamId).Returns(stream);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();

            _ = _streamDeletionService.Received(1).DeleteStreamAsync(stream);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTrueWithoutDeletingStream_When_StreamDoesntExist()
        {
            // Arrange
            var transactionId = _fixture.Create<double>();
            var streamId = _fixture.Create<uint>();
            var commandObject = new Dictionary<string, object>();
            var command = new RtmpDeleteStreamCommand(transactionId, commandObject, streamId);

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStream(streamId).Returns((IRtmpStream?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();

            _clientContext.DidNotReceive().CreateNewStream();

            _ = _streamDeletionService.DidNotReceive().DeleteStreamAsync(Arg.Any<IRtmpStream>());
        }
    }
}
