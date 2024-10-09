using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Media;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.Media
{
    public class RtmpVideoMessageHandlerTest : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpVideoDataProcessorService _videoDataProcessor;
        private readonly ILogger<RtmpVideoMessageHandler> _logger;
        private readonly IDataBuffer _dataBuffer;
        private readonly RtmpVideoMessageHandler _sut;

        public RtmpVideoMessageHandlerTest()
        {
            _fixture = new Fixture();
            _clientContext = Substitute.For<IRtmpClientSessionContext>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _videoDataProcessor = Substitute.For<IRtmpVideoDataProcessorService>();
            _logger = Substitute.For<ILogger<RtmpVideoMessageHandler>>();

            _dataBuffer = new DataBuffer();

            _sut = new RtmpVideoMessageHandler(_videoDataProcessor, _logger);
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
        internal async Task HandleAsync_Should_ProcessVideoData()
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

            _videoDataProcessor.ProcessVideoDataAsync(publisher_publishStreamContext, timestamp, _dataBuffer)
               .Returns(success);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, _dataBuffer, default);

            // Assert
            result.Should().Be(success);

            _ = _videoDataProcessor.Received(1).ProcessVideoDataAsync(publisher_publishStreamContext, timestamp, _dataBuffer);
        }
    }
}
