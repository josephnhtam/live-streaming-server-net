using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Filtering.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpVideoDataProcessorService : IRtmpVideoDataProcessorService
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpMediaMessageBroadcasterService _mediaMessageBroadcaster;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger _logger;

        private readonly IFilter<VideoCodec>? _videoCodecFilter;

        public RtmpVideoDataProcessorService(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpMediaMessageBroadcasterService mediaMessageBroadcaster,
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpVideoDataProcessorService> logger,
            IFilter<VideoCodec>? videoCodecFilter = null)
        {
            _streamManager = streamManager;
            _mediaMessageCacher = mediaMessageCacher;
            _mediaMessageBroadcaster = mediaMessageBroadcaster;
            _config = config.Value;
            _logger = logger;

            _videoCodecFilter = videoCodecFilter;
        }

        public async ValueTask<bool> ProcessVideoDataAsync(IRtmpPublishStreamContext publishStreamContext, uint timestamp, IDataBuffer payloadBuffer)
        {
            ProcessVideoHeader(payloadBuffer);

            var (frameType, videoCodec, avcPacketType) = ParseVideoMessageProperties(payloadBuffer.MoveTo(0));

            if (!IsVideoCodecAllowed(publishStreamContext, videoCodec)) return false;

            await HandleVideoPacketCachingAsync(
                publishStreamContext,
                timestamp,
                frameType,
                avcPacketType,
                payloadBuffer.MoveTo(0));

            await BroadcastVideoMessageToSubscribersAsync(
                publishStreamContext,
                timestamp,
                IsSkippable(avcPacketType),
                payloadBuffer.MoveTo(0));

            return true;
        }

        private async ValueTask BroadcastVideoMessageToSubscribersAsync(
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            bool isSkippable,
            IDataBuffer payloadBuffer)
        {
            var subscribeStreamContexts = _streamManager.GetSubscribeStreamContexts(publishStreamContext.StreamPath);
            await BroadcastVideoMessageToSubscribersAsync(publishStreamContext, timestamp, isSkippable, payloadBuffer, subscribeStreamContexts);
        }

        private async ValueTask BroadcastVideoMessageToSubscribersAsync(
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            bool isSkippable,
            IDataBuffer payloadBuffer,
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            publishStreamContext.UpdateTimestamp(timestamp, MediaType.Video);

            await _mediaMessageBroadcaster.BroadcastMediaMessageAsync(
                publishStreamContext,
                subscribeStreamContexts,
                MediaType.Video,
                timestamp,
                isSkippable,
                payloadBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsVideoCodecAllowed(IRtmpPublishStreamContext publishStreamContext, VideoCodec videoCodec)
        {
            if (_videoCodecFilter != null && !_videoCodecFilter.IsAllowed(videoCodec))
            {
                _logger.VideoCodecNotAllowed(publishStreamContext.StreamPath, videoCodec);
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessVideoHeader(IDataBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var isExHeader = (firstByte >> 7) != 0;

            if (isExHeader)
            {
                ProessExVideoHeader(payloadBuffer, firstByte);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProessExVideoHeader(IDataBuffer payloadBuffer, byte firstByte)
        {
            var frameType = (VideoFrameType)((firstByte >> 4) & 0x7);
            var packetType = (VideoPacketType)(firstByte & 0x0f);
            var fourCC = payloadBuffer.ReadUInt32BigEndian();

            if (fourCC == VideoFourCC.AV1)
            {
                ProcessExVideoHeader(payloadBuffer, frameType, packetType, VideoCodec.AV1);
            }
            else if (fourCC == VideoFourCC.HEVC)
            {
                ProcessExVideoHeader(payloadBuffer, frameType, packetType, VideoCodec.HEVC);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessExVideoHeader(
            IDataBuffer payloadBuffer, VideoFrameType frameType, VideoPacketType packetType, VideoCodec videoCodec)
        {
            var avcPackageType = packetType == VideoPacketType.SequenceStart ? AVCPacketType.SequenceHeader : AVCPacketType.NALU;

            if (videoCodec == VideoCodec.HEVC && packetType == VideoPacketType.CodedFrames)
            {
                RemovePadding(payloadBuffer, packetType, 3);
                RewritePayloadHeader(payloadBuffer, frameType, videoCodec, avcPackageType);
            }
            else
            {
                RewritePayloadHeader(payloadBuffer, frameType, videoCodec, avcPackageType);
                RewriteCompositionTimeOffset(payloadBuffer, 0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void RemovePadding(IDataBuffer payloadBuffer, VideoPacketType packetType, int size)
            {
                payloadBuffer.TrimStart(size);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void RewritePayloadHeader(IDataBuffer payloadBuffer, VideoFrameType frameType, VideoCodec videoCodec, AVCPacketType avcPackageType)
            {
                payloadBuffer.MoveTo(0);
                payloadBuffer.Write((byte)((int)frameType << 4 | (int)videoCodec));
                payloadBuffer.Write((byte)avcPackageType);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void RewriteCompositionTimeOffset(IDataBuffer payloadBuffer, uint compositionTime)
            {
                payloadBuffer.MoveTo(2);
                payloadBuffer.WriteUInt24BigEndian(compositionTime);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (VideoFrameType, VideoCodec, AVCPacketType?) ParseVideoMessageProperties(IDataBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var frameType = (VideoFrameType)((firstByte >> 4) & 0x7);
            var videoCodec = (VideoCodec)(firstByte & 0x0f);

            if (videoCodec is VideoCodec.AVC or VideoCodec.HEVC or VideoCodec.AV1)
            {
                var avcPackageType = (AVCPacketType)payloadBuffer.ReadByte();
                return (frameType, videoCodec, avcPackageType);
            }

            return (frameType, videoCodec, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSkippable(AVCPacketType? avcPacketType)
        {
            return avcPacketType != AVCPacketType.SequenceHeader && avcPacketType != AVCPacketType.EndOfSequence;
        }

        private async ValueTask HandleVideoPacketCachingAsync(
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            VideoFrameType frameType,
            AVCPacketType? avcPacketType,
            IDataBuffer payloadBuffer)
        {
            if (avcPacketType.HasValue)
            {
                publishStreamContext.GroupOfPicturesCacheActivated = _config.EnableGopCaching;

                if (publishStreamContext.GroupOfPicturesCacheActivated && frameType == VideoFrameType.KeyFrame)
                {
                    await _mediaMessageCacher.ClearGroupOfPicturesCacheAsync(publishStreamContext);
                }

                if (frameType == VideoFrameType.KeyFrame && avcPacketType == AVCPacketType.SequenceHeader)
                {
                    await _mediaMessageCacher.CacheSequenceHeaderAsync(publishStreamContext, MediaType.Video, payloadBuffer);
                }
                else if (publishStreamContext.GroupOfPicturesCacheActivated && avcPacketType == AVCPacketType.NALU)
                {
                    await _mediaMessageCacher.CachePictureAsync(publishStreamContext, MediaType.Video, payloadBuffer, timestamp);
                }
            }
            else if (publishStreamContext.GroupOfPicturesCacheActivated)
            {
                publishStreamContext.GroupOfPicturesCacheActivated = false;
                await _mediaMessageCacher.ClearGroupOfPicturesCacheAsync(publishStreamContext);
            }
        }
    }
}
