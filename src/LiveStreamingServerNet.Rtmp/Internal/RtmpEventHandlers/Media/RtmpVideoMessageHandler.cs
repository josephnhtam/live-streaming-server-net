using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        public RtmpVideoMessageHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpMediaMessageBroadcasterService mediaMessageBroadcaster,
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpVideoMessageHandler> logger)
        {
            _streamManager = streamManager;
            _mediaMessageCacher = mediaMessageCacher;
            _mediaMessageBroadcaster = mediaMessageBroadcaster;
            _config = config.Value;
            _logger = logger;
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

            var hasHeader = await CacheVideoSequenceAsync(chunkStreamContext, publishStreamContext, payloadBuffer);
            await BroacastVideoMessageToSubscribersAsync(chunkStreamContext, publishStreamContext, payloadBuffer, hasHeader);
            return true;
        }

        private async ValueTask BroacastVideoMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer,
            bool hasSequenceHeader)
        {
            if (hasSequenceHeader)
            {
                using var subscribers = _streamManager.GetSubscribersLocked(publishStreamContext.StreamPath);
                await BroacastVideoMessageToSubscribersAsync(chunkStreamContext, publishStreamContext, false, payloadBuffer, subscribers.Value);
            }
            else
            {
                var subscribers = _streamManager.GetSubscribers(publishStreamContext.StreamPath);
                await BroacastVideoMessageToSubscribersAsync(chunkStreamContext, publishStreamContext, true, payloadBuffer, subscribers);
            }
        }

        private async ValueTask BroacastVideoMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            bool isSkippable,
            INetBuffer payloadBuffer,
            IReadOnlyList<IRtmpClientContext> subscribers)
        {
            await _mediaMessageBroadcaster.BroadcastMediaMessageAsync(
                publishStreamContext,
                subscribers,
                MediaType.Video,
                chunkStreamContext.MessageHeader.Timestamp,
                isSkippable,
                payloadBuffer);
        }

        private async ValueTask<bool> CacheVideoSequenceAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var frameType = (VideoFrameType)(firstByte >> 4);
            var codecId = (VideoCodecId)(firstByte & 0x0f);

            if (codecId is VideoCodecId.AVC or VideoCodecId.HVC or VideoCodecId.Opus)
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

            payloadBuffer.MoveTo(0);

            return false;
        }
    }
}
