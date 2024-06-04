using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Commands
{
    public class RtmpDeleteStreamCommandHandlerTest
    {
        private readonly IFixture _fixture;
        private readonly IRtmpStreamDeletionService _streamDeletionService;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpClientContext _clientContext;
        private readonly RtmpDeleteStreamCommandHandler _sut;

        public RtmpDeleteStreamCommandHandlerTest()
        {
            _fixture = new Fixture();
            _streamDeletionService = Substitute.For<IRtmpStreamDeletionService>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _clientContext = Substitute.For<IRtmpClientContext>();

            _sut = new RtmpDeleteStreamCommandHandler(_streamDeletionService);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTrueAndDeleteStream_When_StreamIdsMatch()
        {
            // Arrange
            var transactionId = _fixture.Create<double>();
            var streamId = _fixture.Create<uint>();
            var commandObject = new Dictionary<string, object>();
            var command = new RtmpDeleteStreamCommand(transactionId, commandObject, streamId);

            _clientContext.StreamId.Returns(streamId);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();

            _ = _streamDeletionService.Received(1).DeleteStreamAsync(_clientContext);
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnTrueWithoutDeletingStream_When_StreamIdsDoNotMatch()
        {
            // Arrange
            var transactionId = _fixture.Create<double>();
            var streamId = _fixture.Create<uint>();
            var commandObject = new Dictionary<string, object>();
            var command = new RtmpDeleteStreamCommand(transactionId, commandObject, streamId);

            _clientContext.StreamId.Returns((uint?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, command, default);

            // Assert
            result.Should().BeTrue();

            _clientContext.DidNotReceive().CreateNewStream();

            _ = _streamDeletionService.DidNotReceive().DeleteStreamAsync(_clientContext);
        }
    }
}
