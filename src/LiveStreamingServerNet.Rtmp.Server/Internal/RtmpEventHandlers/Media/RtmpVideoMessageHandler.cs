using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Filtering.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Media
{
    [RtmpMessageType(RtmpMessageType.VideoMessage)]
    internal class RtmpVideoMessageHandler : IRtmpMessageHandler<IRtmpClientSessionContext>
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpMediaMessageBroadcasterService _mediaMessageBroadcaster;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger _logger;

        private readonly IFilter<VideoCodec>? _videoCodecFilter;

        public RtmpVideoMessageHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpMediaMessageBroadcasterService mediaMessageBroadcaster,
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpVideoMessageHandler> logger,
            IFilter<VideoCodec>? videoCodecFilter = null)
        {
            _streamManager = streamManager;
            _mediaMessageCacher = mediaMessageCacher;
            _mediaMessageBroadcaster = mediaMessageBroadcaster;
            _config = config.Value;
            _logger = logger;

            _videoCodecFilter = videoCodecFilter;
        }

        public async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var publishStreamContext = clientContext.GetStreamContext(streamId)?.PublishContext;

            if (publishStreamContext == null)
            {
                _logger.PublishStreamNotYetCreated(clientContext.Client.Id, streamId);
                return false;
            }

            ProcessVideoHeader(payloadBuffer);

            var (frameType, videoCodec, avcPacketType) = ParseVideoMessageProperties(payloadBuffer.MoveTo(0));

            if (!IsVideoCodecAllowed(clientContext, publishStreamContext, videoCodec)) return false;

            await HandleVideoPacketCachingAsync(
                chunkStreamContext,
                publishStreamContext,
                frameType,
                avcPacketType,
                payloadBuffer.MoveTo(0));

            await BroadcastVideoMessageToSubscribersAsync(
                chunkStreamContext,
                publishStreamContext,
                payloadBuffer.MoveTo(0),
                IsSkippable(avcPacketType));

            return true;
        }

        private async ValueTask BroadcastVideoMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            IDataBuffer payloadBuffer,
            bool isSkippable)
        {
            var subscribeStreamContexts = _streamManager.GetSubscribeStreamContexts(publishStreamContext.StreamPath);
            await BroadcastVideoMessageToSubscribersAsync(chunkStreamContext, publishStreamContext, isSkippable, payloadBuffer, subscribeStreamContexts);
        }

        private async ValueTask BroadcastVideoMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            bool isSkippable,
            IDataBuffer payloadBuffer,
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            publishStreamContext.UpdateTimestamp(chunkStreamContext.Timestamp, MediaType.Video);

            await _mediaMessageBroadcaster.BroadcastMediaMessageAsync(
                publishStreamContext,
                subscribeStreamContexts,
                MediaType.Video,
                chunkStreamContext.Timestamp,
                isSkippable,
                payloadBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsVideoCodecAllowed(IRtmpClientSessionContext clientContext, IRtmpPublishStreamContext publishStreamContext, VideoCodec videoCodec)
        {
            if (_videoCodecFilter != null && !_videoCodecFilter.IsAllowed(videoCodec))
            {
                _logger.VideoCodecNotAllowed(clientContext.Client.Id, publishStreamContext.StreamPath, videoCodec);
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
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
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
                    await _mediaMessageCacher.CachePictureAsync(publishStreamContext, MediaType.Video, payloadBuffer, chunkStreamContext.Timestamp);
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
