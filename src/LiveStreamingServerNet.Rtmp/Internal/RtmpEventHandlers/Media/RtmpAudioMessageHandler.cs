using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Media
{
    [RtmpMessageType(RtmpMessageType.AudioMessage)]
    internal class RtmpAudioMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpMediaMessageBroadcasterService _mediaMessageBroadcaster;
        private readonly ILogger _logger;

        public RtmpAudioMessageHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpMediaMessageBroadcasterService mediaMessageBroadcaster,
            ILogger<RtmpAudioMessageHandler> logger)
        {
            _streamManager = streamManager;
            _mediaMessageCacher = mediaMessageCacher;
            _mediaMessageBroadcaster = mediaMessageBroadcaster;
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

            var hasHeader = await CacheAudioSequenceAsync(chunkStreamContext, publishStreamContext, payloadBuffer);
            await BroadcastAudioMessageToSubscribersAsync(chunkStreamContext, clientContext, publishStreamContext, payloadBuffer, hasHeader);
            return true;
        }

        private async ValueTask BroadcastAudioMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer,
            bool hasSequenceHeader)
        {
            var subscribers = _streamManager.GetSubscribers(publishStreamContext.StreamPath);
            await BroadcastAudioMessageToSubscribersAsync(chunkStreamContext, clientContext, publishStreamContext, !hasSequenceHeader, payloadBuffer, subscribers);
        }

        private async ValueTask BroadcastAudioMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            bool isSkippable,
            INetBuffer payloadBuffer,
            IReadOnlyList<IRtmpClientContext> subscribers)
        {
            clientContext.UpdateTimestamp(chunkStreamContext.MessageHeader.Timestamp, MediaType.Audio);

            await _mediaMessageBroadcaster.BroadcastMediaMessageAsync(
                publishStreamContext,
                subscribers,
                MediaType.Audio,
                chunkStreamContext.MessageHeader.Timestamp,
                isSkippable,
                payloadBuffer);
        }

        private async ValueTask<bool> CacheAudioSequenceAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var soundFormat = (AudioSoundFormat)(firstByte >> 4);

            if (soundFormat is AudioSoundFormat.AAC or AudioSoundFormat.Opus)
            {
                var aacPackageType = (AACPacketType)payloadBuffer.ReadByte();
                if (aacPackageType == AACPacketType.SequenceHeader)
                {
                    await _mediaMessageCacher.CacheSequenceHeaderAsync(publishStreamContext, MediaType.Audio, payloadBuffer);
                    return true;
                }
                else if (publishStreamContext.GroupOfPicturesCacheActivated)
                {
                    await _mediaMessageCacher.CachePictureAsync(publishStreamContext, MediaType.Audio, payloadBuffer, chunkStreamContext.MessageHeader.Timestamp);
                }
            }

            payloadBuffer.MoveTo(0);
            return false;
        }
    }
}
