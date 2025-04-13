using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace LiveStreamingServerNet.Rtmp.Server.Test.Services
{
    public class RtmpVideoDataProcessorServiceTest : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly IRtmpClientSessionContext _clientContext;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCacherService _cacher;
        private readonly IRtmpMediaMessageBroadcasterService _mediaMessageBroadcaster;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger<RtmpVideoDataProcessorService> _logger;
        private readonly IDataBuffer _dataBuffer;
        private readonly IRtmpVideoDataProcessorService _sut;

        public RtmpVideoDataProcessorServiceTest()
        {
            _fixture = new Fixture();
            _clientContext = Substitute.For<IRtmpClientSessionContext>();
            _streamManager = Substitute.For<IRtmpStreamManagerService>();
            _cacher = Substitute.For<IRtmpCacherService>();
            _mediaMessageBroadcaster = Substitute.For<IRtmpMediaMessageBroadcasterService>();
            _config = new RtmpServerConfiguration();
            _logger = Substitute.For<ILogger<RtmpVideoDataProcessorService>>();

            _dataBuffer = new DataBuffer();

            _sut = new RtmpVideoDataProcessorService(
                _streamManager,
                _cacher,
                _mediaMessageBroadcaster,
                Options.Create(_config),
                _logger);
        }

        public void Dispose()
        {
            _dataBuffer.Dispose();
        }

        [Theory]
        [InlineData(true, VideoFrameType.KeyFrame, VideoCodec.AVC, AVCPacketType.SequenceHeader)]
        [InlineData(true, VideoFrameType.InterFrame, VideoCodec.AVC, AVCPacketType.NALU)]
        [InlineData(true, VideoFrameType.KeyFrame, VideoCodec.HEVC, AVCPacketType.SequenceHeader)]
        [InlineData(true, VideoFrameType.InterFrame, VideoCodec.HEVC, AVCPacketType.NALU)]
        [InlineData(true, VideoFrameType.KeyFrame, VideoCodec.AV1, AVCPacketType.SequenceHeader)]
        [InlineData(true, VideoFrameType.InterFrame, VideoCodec.AV1, AVCPacketType.NALU)]
        [InlineData(false, VideoFrameType.KeyFrame, VideoCodec.AVC, AVCPacketType.SequenceHeader)]
        [InlineData(false, VideoFrameType.InterFrame, VideoCodec.AVC, AVCPacketType.NALU)]
        [InlineData(false, VideoFrameType.KeyFrame, VideoCodec.HEVC, AVCPacketType.SequenceHeader)]
        [InlineData(false, VideoFrameType.InterFrame, VideoCodec.HEVC, AVCPacketType.NALU)]
        [InlineData(false, VideoFrameType.KeyFrame, VideoCodec.AV1, AVCPacketType.SequenceHeader)]
        [InlineData(false, VideoFrameType.InterFrame, VideoCodec.AV1, AVCPacketType.NALU)]
        internal async Task HandleAsync_Should_HandleCacheAndBroadcastAndReturnTrue(
            bool gopCacheActivated, VideoFrameType frameType, VideoCodec videoCodec, AVCPacketType avcPacketType)
        {
            // Arrange
            _config.EnableGopCaching = gopCacheActivated;

            var stremaPath = _fixture.Create<string>();
            var streamId = _fixture.Create<uint>();
            var timestamp = _fixture.Create<uint>();

            var subscriber_subscribeStreamContext = Substitute.For<IRtmpSubscribeStreamContext>();
            var subscriber_subscribeStreamContexts = new List<IRtmpSubscribeStreamContext>() { subscriber_subscribeStreamContext };

            var publisher_streamContext = Substitute.For<IRtmpStreamContext>();
            var publisher_publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();

            publisher_streamContext.StreamId.Returns(streamId);
            publisher_streamContext.ClientContext.Returns(_clientContext);
            publisher_streamContext.PublishContext.Returns(publisher_publishStreamContext);
            publisher_publishStreamContext.StreamPath.Returns(stremaPath);
            publisher_publishStreamContext.StreamContext.Returns(publisher_streamContext);

            _clientContext.GetStreamContext(streamId).Returns(publisher_streamContext);
            _streamManager.GetSubscribeStreamContexts(stremaPath).Returns(subscriber_subscribeStreamContexts);

            var firstByte = (byte)((byte)frameType << 4 | (byte)videoCodec);
            _dataBuffer.Write(firstByte);
            _dataBuffer.Write((byte)avcPacketType);
            _dataBuffer.Write(_fixture.Create<byte[]>());
            _dataBuffer.MoveTo(0);

            var hasHeader =
                videoCodec is VideoCodec.AVC or VideoCodec.HEVC or VideoCodec.AV1 &&
                avcPacketType is AVCPacketType.SequenceHeader &&
                frameType is VideoFrameType.KeyFrame;

            var isPictureCachable =
                videoCodec is VideoCodec.AVC or VideoCodec.HEVC or VideoCodec.AV1 &&
                avcPacketType is AVCPacketType.NALU;

            var isSkippable = !hasHeader;

            // Act
            var result = await _sut.ProcessVideoDataAsync(publisher_publishStreamContext, timestamp, _dataBuffer);

            // Assert
            result.Should().BeTrue();

            publisher_publishStreamContext.Received(1).UpdateTimestamp(timestamp, MediaType.Video);

            if (gopCacheActivated && frameType == VideoFrameType.KeyFrame)
                _ = _cacher.Received(1).ClearGroupOfPicturesCacheAsync(publisher_publishStreamContext);

            _ = _cacher.Received(hasHeader ? 1 : 0)
                .CacheSequenceHeaderAsync(publisher_publishStreamContext, MediaType.Video, _dataBuffer);

            _ = _cacher.Received(gopCacheActivated && isPictureCachable ? 1 : 0)
                .CachePictureAsync(publisher_publishStreamContext, MediaType.Video, _dataBuffer, publisher_publishStreamContext.VideoTimestamp);

            await _mediaMessageBroadcaster.Received(1).BroadcastMediaMessageAsync(
                publisher_publishStreamContext,
                subscriber_subscribeStreamContexts,
                MediaType.Video,
                publisher_publishStreamContext.VideoTimestamp,
                isSkippable,
                _dataBuffer
            );
        }
    }
}
