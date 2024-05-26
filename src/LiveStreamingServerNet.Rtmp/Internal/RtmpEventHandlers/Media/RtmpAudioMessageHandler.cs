using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Filtering.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Media
{
    [RtmpMessageType(RtmpMessageType.AudioMessage)]
    internal class RtmpAudioMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpMediaMessageBroadcasterService _mediaMessageBroadcaster;
        private readonly ILogger _logger;

        private readonly IFilter<AudioCodec>? _audioCodecFilter;

        public RtmpAudioMessageHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpMediaMessageBroadcasterService mediaMessageBroadcaster,
            ILogger<RtmpAudioMessageHandler> logger,
            IFilter<AudioCodec>? audioCodecFilter = null)
        {
            _streamManager = streamManager;
            _mediaMessageCacher = mediaMessageCacher;
            _mediaMessageBroadcaster = mediaMessageBroadcaster;
            _logger = logger;

            _audioCodecFilter = audioCodecFilter;
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

            var audioCodec = ParseAudioMessageProperties(payloadBuffer);
            if (!IsAudioCodecAllowed(clientContext, publishStreamContext, audioCodec)) return false;

            var hasHeader = await HandleAudioSequenceHeaderAsync(chunkStreamContext, publishStreamContext, audioCodec, payloadBuffer);
            await BroadcastAudioMessageToSubscribersAsync(chunkStreamContext, clientContext, publishStreamContext, payloadBuffer.MoveTo(0), hasHeader);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsAudioCodecAllowed(IRtmpClientContext clientContext, IRtmpPublishStreamContext publishStreamContext, AudioCodec audioCodec)
        {
            if (_audioCodecFilter != null && !_audioCodecFilter.IsAllowed(audioCodec))
            {
                _logger.AudioCodecNotAllowed(clientContext.Client.ClientId, publishStreamContext.StreamPath, audioCodec);
                return false;
            }

            return true;
        }

        private AudioCodec ParseAudioMessageProperties(INetBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var audioCodec = (AudioCodec)(firstByte >> 4);

            if (_audioCodecFilter != null && !_audioCodecFilter.IsAllowed(audioCodec))
                return audioCodec;

            return audioCodec;
        }

        private async ValueTask<bool> HandleAudioSequenceHeaderAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            AudioCodec audioCodec,
            INetBuffer payloadBuffer)
        {
            if (audioCodec is AudioCodec.AAC or AudioCodec.Opus)
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

            return false;
        }
    }
}
