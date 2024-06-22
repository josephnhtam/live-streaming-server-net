using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Media;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.Media
{
    public class RtmpVideoMessageHandlerTest : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly IRtmpClientContext _clientContext;
        private readonly IRtmpChunkStreamContext _chunkStreamContext;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpMediaMessageBroadcasterService _mediaMessageBroadcaster;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger<RtmpVideoMessageHandler> _logger;
        private readonly IDataBuffer _dataBuffer;
        private readonly RtmpVideoMessageHandler _sut;

        public RtmpVideoMessageHandlerTest()
        {
            _fixture = new Fixture();
            _clientContext = Substitute.For<IRtmpClientContext>();
            _chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            _streamManager = Substitute.For<IRtmpStreamManagerService>();
            _mediaMessageCacher = Substitute.For<IRtmpMediaMessageCacherService>();
            _mediaMessageBroadcaster = Substitute.For<IRtmpMediaMessageBroadcasterService>();
            _config = new RtmpServerConfiguration();
            _logger = Substitute.For<ILogger<RtmpVideoMessageHandler>>();

            _dataBuffer = new DataBuffer();

            _sut = new RtmpVideoMessageHandler(
                _streamManager,
                _mediaMessageCacher,
                _mediaMessageBroadcaster,
                Options.Create(_config),
                _logger);
        }

        public void Dispose()
        {
            _dataBuffer.Dispose();
        }

        [Fact]
        public async Task HandleAsync_Should_ReturnFalse_When_StreamNotYetCreated()
        {
            // Arrange
            _clientContext.PublishStreamContext.Returns((IRtmpPublishStreamContext?)null);

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, _dataBuffer, default);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(true, VideoFrameType.KeyFrame, VideoCodec.AVC, AVCPacketType.SequenceHeader)]
        [InlineData(true, VideoFrameType.InterFrame, VideoCodec.AVC, AVCPacketType.NALU)]
        [InlineData(true, VideoFrameType.KeyFrame, VideoCodec.HVC, AVCPacketType.SequenceHeader)]
        [InlineData(true, VideoFrameType.InterFrame, VideoCodec.HVC, AVCPacketType.NALU)]
        [InlineData(true, VideoFrameType.KeyFrame, VideoCodec.Opus, AVCPacketType.SequenceHeader)]
        [InlineData(true, VideoFrameType.InterFrame, VideoCodec.Opus, AVCPacketType.NALU)]
        [InlineData(false, VideoFrameType.KeyFrame, VideoCodec.AVC, AVCPacketType.SequenceHeader)]
        [InlineData(false, VideoFrameType.InterFrame, VideoCodec.AVC, AVCPacketType.NALU)]
        [InlineData(false, VideoFrameType.KeyFrame, VideoCodec.HVC, AVCPacketType.SequenceHeader)]
        [InlineData(false, VideoFrameType.InterFrame, VideoCodec.HVC, AVCPacketType.NALU)]
        [InlineData(false, VideoFrameType.KeyFrame, VideoCodec.Opus, AVCPacketType.SequenceHeader)]
        [InlineData(false, VideoFrameType.InterFrame, VideoCodec.Opus, AVCPacketType.NALU)]
        internal async Task HandleAsync_Should_HandleCacheAndBroadcastAndReturnTrue(
            bool gopCacheActivated, VideoFrameType frameType, VideoCodec videoCodec, AVCPacketType avcPacketType)
        {
            // Arrange
            _config.EnableGopCaching = gopCacheActivated;

            var stremaPath = _fixture.Create<string>();

            var subscriber = Substitute.For<IRtmpClientContext>();
            var subscribers = new List<IRtmpClientContext>() { subscriber };

            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            publishStreamContext.StreamPath.Returns(stremaPath);

            _clientContext.PublishStreamContext.Returns(publishStreamContext);
            _streamManager.GetSubscribers(stremaPath).Returns(subscribers);

            var firstByte = (byte)((byte)frameType << 4 | (byte)videoCodec);
            _dataBuffer.Write(firstByte);
            _dataBuffer.Write((byte)avcPacketType);
            _dataBuffer.Write(_fixture.Create<byte[]>());
            _dataBuffer.MoveTo(0);

            var hasHeader =
                (videoCodec is VideoCodec.AVC or VideoCodec.HVC or VideoCodec.Opus) &&
                avcPacketType is AVCPacketType.SequenceHeader &&
                frameType is VideoFrameType.KeyFrame;

            var isPictureCachable =
                (videoCodec is VideoCodec.AVC or VideoCodec.HVC or VideoCodec.Opus) &&
                avcPacketType is AVCPacketType.NALU;

            var isSkippable = !hasHeader;

            // Act
            var result = await _sut.HandleAsync(_chunkStreamContext, _clientContext, _dataBuffer, default);

            // Assert
            result.Should().BeTrue();

            if (gopCacheActivated && frameType == VideoFrameType.KeyFrame)
                _ = _mediaMessageCacher.Received(1).ClearGroupOfPicturesCacheAsync(publishStreamContext);

            _ = _mediaMessageCacher.Received(hasHeader ? 1 : 0)
                .CacheSequenceHeaderAsync(publishStreamContext, MediaType.Video, _dataBuffer);

            _ = _mediaMessageCacher.Received(gopCacheActivated && isPictureCachable ? 1 : 0)
                .CachePictureAsync(publishStreamContext, MediaType.Video, _dataBuffer, _chunkStreamContext.MessageHeader.Timestamp);

            _clientContext.Received(1).UpdateTimestamp(_chunkStreamContext.MessageHeader.Timestamp, MediaType.Video);

            await _mediaMessageBroadcaster.Received(1).BroadcastMediaMessageAsync(
                publishStreamContext,
                subscribers,
                MediaType.Video,
                _chunkStreamContext.MessageHeader.Timestamp,
                isSkippable,
                _dataBuffer
            );
        }
    }
}
