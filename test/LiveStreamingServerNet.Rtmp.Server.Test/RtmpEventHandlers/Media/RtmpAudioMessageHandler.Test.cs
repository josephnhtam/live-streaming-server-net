using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Media;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.Media
{
    public class RtmpAudioMessageHandlerTest : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpAudioDataProcessorService _audioDataProcessor;
        private readonly ILogger<RtmpAudioMessageHandler> _logger;
        private readonly IDataBuffer _dataBuffer;
        private readonly RtmpAudioMessageHandler _sut;

        public RtmpAudioMessageHandlerTest()
        {
            _fixture = new Fixture();
            _clientContext = Substitute.For<IRtmpClientSessionContext>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _audioDataProcessor = Substitute.For<IRtmpAudioDataProcessorService>();
            _logger = Substitute.For<ILogger<RtmpAudioMessageHandler>>();

            _dataBuffer = new DataBuffer();

            _sut = new RtmpAudioMessageHandler(_audioDataProcessor, _logger);
        }

        public void Dispose()
        {
            _dataBuffer.Dispose();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnFalse_When_StreamNotYetCreated()
        {
            // Arrange
            _clientContext.GetStreamContext(Arg.Any<uint>()).Returns((IRtmpStreamContext?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, _dataBuffer, default);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        internal async Task HandleAsync_Should_ProcessAudioData()
        {
            // Arrange
            var stremaPath = _fixture.Create<string>();
            var streamId = _fixture.Create<uint>();
            var timestamp = _fixture.Create<uint>();
            var success = _fixture.Create<bool>();

            var publisher_streamContext = Substitute.For<IRtmpStreamContext>();
            var publisher_publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();

            publisher_streamContext.StreamId.Returns(streamId);
            publisher_streamContext.ClientContext.Returns(_clientContext);
            publisher_streamContext.PublishContext.Returns(publisher_publishStreamContext);
            publisher_publishStreamContext.StreamPath.Returns(stremaPath);
            publisher_publishStreamContext.StreamContext.Returns(publisher_streamContext);

            _clientContext.GetStreamContext(streamId).Returns(publisher_streamContext);
            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _chunkStreamContext.Timestamp.Returns(timestamp);

            _audioDataProcessor.ProcessAudioDataAsync(publisher_publishStreamContext, timestamp, _dataBuffer)
                .Returns(success);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, _dataBuffer, default);

            // Assert
            result.Should().Be(success);

            _ = _audioDataProcessor.Received(1).ProcessAudioDataAsync(publisher_publishStreamContext, timestamp, _dataBuffer);
        }
    }
}
