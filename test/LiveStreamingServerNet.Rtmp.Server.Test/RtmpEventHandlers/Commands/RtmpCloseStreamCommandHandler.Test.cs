using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.Commands
{
    public class RtmpCloseStreamCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpStreamDeletionService _streamDeletionService;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly RtmpCloseStreamCommandHandler _sut;

        public RtmpCloseStreamCommandHandlerTest()
        {
            _fixture = new Fixture();
            _streamDeletionService = Substitute.For<IRtmpStreamDeletionService>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientSessionContext>();
            _sut = new RtmpCloseStreamCommandHandler(_streamDeletionService);
        }

        [Fact]
        public async Task HandleAsync_Should_CloseStream_When_StreamExists()
        {
            // Arrange
            var command = new RtmpCloseStreamCommand(0, new Dictionary<string, object>());
            var streamContext = Substitute.For<IRtmpStreamContext>();
            var streamId = _fixture.Create<uint>();

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStreamContext(streamId).Returns(streamContext);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();
            _ = _streamDeletionService.Received(1).CloseStreamAsync(streamContext);
        }

        [Fact]
        public async Task HandleAsync_Should_NotCloseStream_When_StreamDoesntExist()
        {
            // Arrange
            var command = new RtmpCloseStreamCommand(0, new Dictionary<string, object>());
            var streamId = _fixture.Create<uint>();

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStreamContext(streamId).Returns((IRtmpStreamContext?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();
            _ = _streamDeletionService.DidNotReceive().CloseStreamAsync(Arg.Any<IRtmpStreamContext>());
        }
    }
}
