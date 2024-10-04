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
            _clientContext = Substitute.For<IRtmpClientSessionContext>();
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
            _clientContext.GetStreamContext(Arg.Any<uint>()).Returns((IRtmpStreamContext?)null);

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
            var streamId = _fixture.Create<uint>();

            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscriber_subscribeStreamContexts = new List<IRtmpSubscribeStreamContext>() { subscriber_subscribeStreamContext };

            var publisher_streamContext = Substitute.For<IRtmpStreamContext>();
            var publisher_publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();

            publisher_streamContext.StreamId.Returns(streamId);
            publisher_streamContext.ClientContext.Returns(_clientContext);
            publisher_streamContext.PublishContext.Returns(publisher_publishStreamContext);
            publisher_publishStreamContext.StreamPath.Returns(stremaPath);
            publisher_publishStreamContext.StreamContext.Returns(publisher_streamContext);

            _chunkStreamContext.MessageHeader.MessageStreamId.Returns(streamId);
            _clientContext.GetStreamContext(streamId).Returns(publisher_streamContext);
            _streamManager.GetSubscribeStreamContexts(stremaPath).Returns(subscriber_subscribeStreamContexts);

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
                _ = _mediaMessageCacher.Received(1).ClearGroupOfPicturesCacheAsync(publisher_publishStreamContext);

            _ = _mediaMessageCacher.Received(hasHeader ? 1 : 0)
                .CacheSequenceHeaderAsync(publisher_publishStreamContext, MediaType.Video, _dataBuffer);

            _ = _mediaMessageCacher.Received(gopCacheActivated && isPictureCachable ? 1 : 0)
                .CachePictureAsync(publisher_publishStreamContext, MediaType.Video, _dataBuffer, _chunkStreamContext.Timestamp);

            publisher_publishStreamContext.Received(1).UpdateTimestamp(_chunkStreamContext.Timestamp, MediaType.Video);

            await _mediaMessageBroadcaster.Received(1).BroadcastMediaMessageAsync(
                publisher_publishStreamContext,
                subscriber_subscribeStreamContexts,
                MediaType.Video,
                _chunkStreamContext.Timestamp,
                isSkippable,
                _dataBuffer
            );
        }
    }
}
