using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Filtering.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
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
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var publishStreamContext = clientContext.PublishStreamContext;

            if (publishStreamContext == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.ClientId);
                return false;
            }

            var (frameType, videoCodec) = ParseVideoMessageProperties(payloadBuffer);
            if (!IsVideoCodecAllowed(clientContext, publishStreamContext, videoCodec)) return false;

            var hasHeader = await HandleVideoSequenceHeaderAsync(chunkStreamContext, publishStreamContext, frameType, videoCodec, payloadBuffer);
            await BroadcastVideoMessageToSubscribersAsync(chunkStreamContext, clientContext, publishStreamContext, payloadBuffer.MoveTo(0), hasHeader);

            return true;
        }

        private async ValueTask BroadcastVideoMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer,
            bool hasSequenceHeader)
        {
            var subscribers = _streamManager.GetSubscribers(publishStreamContext.StreamPath);
            await BroadcastVideoMessageToSubscribersAsync(chunkStreamContext, clientContext, publishStreamContext, !hasSequenceHeader, payloadBuffer, subscribers);
        }

        private async ValueTask BroadcastVideoMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            bool isSkippable,
            INetBuffer payloadBuffer,
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

        private (VideoFrameType, VideoCodec) ParseVideoMessageProperties(INetBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var frameType = (VideoFrameType)(firstByte >> 4);
            var videoCodec = (VideoCodec)(firstByte & 0x0f);

            return (frameType, videoCodec);
        }

        private async ValueTask<bool> HandleVideoSequenceHeaderAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            VideoFrameType frameType,
            VideoCodec videoCodec,
            INetBuffer payloadBuffer)
        {
            if (videoCodec is VideoCodec.AVC or VideoCodec.HVC or VideoCodec.Opus)
            {
                publishStreamContext.GroupOfPicturesCacheActivated = _config.EnableGopCaching;
                var avcPackageType = (AVCPacketType)payloadBuffer.ReadByte();

                if (publishStreamContext.GroupOfPicturesCacheActivated && frameType == VideoFrameType.KeyFrame)
                {
                    await _mediaMessageCacher.ClearGroupOfPicturesCacheAsync(publishStreamContext);
                }

                if (frameType == VideoFrameType.KeyFrame && avcPackageType == AVCPacketType.SequenceHeader)
                {
                    await _mediaMessageCacher.CacheSequenceHeaderAsync(publishStreamContext, MediaType.Video, payloadBuffer);
                    return true;
                }

                if (publishStreamContext.GroupOfPicturesCacheActivated && avcPackageType == AVCPacketType.NALU)
                {
                    await _mediaMessageCacher.CachePictureAsync(publishStreamContext, MediaType.Video, payloadBuffer, chunkStreamContext.MessageHeader.Timestamp);
                }
            }
            else if (publishStreamContext.GroupOfPicturesCacheActivated)
            {
                publishStreamContext.GroupOfPicturesCacheActivated = false;
                await _mediaMessageCacher.ClearGroupOfPicturesCacheAsync(publishStreamContext);
            }

            return false;
        }
    }
}
