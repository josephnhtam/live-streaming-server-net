using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Filtering.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Media
{
    [RtmpMessageType(RtmpMessageType.VideoMessage)]
    internal class RtmpVideoMessageHandler : IRtmpMessageHandler
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
            IRtmpClientContext clientContext,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var publishStreamContext = clientContext.PublishStreamContext;

            if (publishStreamContext == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.ClientId);
                return false;
            }

            var (frameType, videoCodec, avcPacketType) = ParseVideoMessageProperties(payloadBuffer);

            if (!IsVideoCodecAllowed(clientContext, publishStreamContext, videoCodec)) return false;

            await HandleVideoPacketCachingAsync(
                chunkStreamContext,
                publishStreamContext,
                frameType,
                avcPacketType,
                payloadBuffer.MoveTo(0));

            await BroadcastVideoMessageToSubscribersAsync(
                chunkStreamContext,
                clientContext,
                publishStreamContext,
                payloadBuffer.MoveTo(0),
                IsSkippable(avcPacketType));

            return true;
        }

        private async ValueTask BroadcastVideoMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            IDataBuffer payloadBuffer,
            bool isSkippable)
        {
            var subscribers = _streamManager.GetSubscribers(publishStreamContext.StreamPath);
            await BroadcastVideoMessageToSubscribersAsync(chunkStreamContext, clientContext, publishStreamContext, isSkippable, payloadBuffer, subscribers);
        }

        private async ValueTask BroadcastVideoMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            bool isSkippable,
            IDataBuffer payloadBuffer,
            IReadOnlyList<IRtmpClientContext> subscribers)
        {
            clientContext.UpdateTimestamp(chunkStreamContext.MessageHeader.Timestamp, MediaType.Video);

            await _mediaMessageBroadcaster.BroadcastMediaMessageAsync(
                publishStreamContext,
                subscribers,
                MediaType.Video,
                chunkStreamContext.MessageHeader.Timestamp,
                isSkippable,
                payloadBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsVideoCodecAllowed(IRtmpClientContext clientContext, IRtmpPublishStreamContext publishStreamContext, VideoCodec videoCodec)
        {
            if (_videoCodecFilter != null && !_videoCodecFilter.IsAllowed(videoCodec))
            {
                _logger.VideoCodecNotAllowed(clientContext.Client.ClientId, publishStreamContext.StreamPath, videoCodec);
                return false;
            }

            return true;
        }

        private (VideoFrameType, VideoCodec, AVCPacketType?) ParseVideoMessageProperties(IDataBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var frameType = (VideoFrameType)(firstByte >> 4);
            var videoCodec = (VideoCodec)(firstByte & 0x0f);

            if (videoCodec is VideoCodec.AVC or VideoCodec.HVC or VideoCodec.Opus)
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
                    await _mediaMessageCacher.CachePictureAsync(publishStreamContext, MediaType.Video, payloadBuffer, chunkStreamContext.MessageHeader.Timestamp);
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
