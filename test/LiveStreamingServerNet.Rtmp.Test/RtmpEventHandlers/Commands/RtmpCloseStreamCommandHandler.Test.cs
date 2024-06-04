using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Commands
{
    public class RtmpCloseStreamCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpStreamDeletionService _streamDeletionService;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientContext _clientContext;
        private readonly RtmpCloseStreamCommandHandler _sut;

        public RtmpCloseStreamCommandHandlerTest()
        {
            _fixture = new Fixture();
            _streamDeletionService = Substitute.For<IRtmpStreamDeletionService>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientContext>();
            _sut = new RtmpCloseStreamCommandHandler(_streamDeletionService);
        }

        [Fact]
        public async Task HandleAsync_Should_DeletesStream_When_StreamIdIsNotNull()
        {
            // Arrange
            var command = new RtmpCloseStreamCommand(0, new Dictionary<string, object>());
            var streamId = _fixture.Create<uint>();
            _clientContext.StreamId.Returns(streamId);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();
            _ = _streamDeletionService.Received(1).DeleteStreamAsync(_clientContext);
        }

        [Fact]
        public async Task HandleAsync_Should_NotDeletesStream_When_StreamIdIsNotNull()
        {
            // Arrange
            var command = new RtmpCloseStreamCommand(0, new Dictionary<string, object>());
            uint? streamId = null;
            _clientContext.StreamId.Returns(streamId);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();
            _ = _streamDeletionService.DidNotReceive().DeleteStreamAsync(Arg.Any<IRtmpClientContext>());
        }
    }
}
