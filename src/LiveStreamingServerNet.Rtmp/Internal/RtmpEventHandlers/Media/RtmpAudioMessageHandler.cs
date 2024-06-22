using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Filtering.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
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
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var publishStreamContext = clientContext.PublishStreamContext;

            if (publishStreamContext == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.ClientId);
                return false;
            }

            var (audioCodec, aacPacketType) = ParseAudioMessageProperties(payloadBuffer);

            if (!IsAudioCodecAllowed(clientContext, publishStreamContext, audioCodec)) return false;

            await HandleAudioPacketCachingAsync(
                chunkStreamContext,
                publishStreamContext,
                aacPacketType,
                payloadBuffer.MoveTo(0));

            await BroadcastAudioMessageToSubscribersAsync(
                chunkStreamContext,
                clientContext,
                publishStreamContext,
                payloadBuffer.MoveTo(0),
                IsSkippable(aacPacketType));

            return true;
        }

        private async ValueTask BroadcastAudioMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            IDataBuffer payloadBuffer,
            bool isSkippable)
        {
            var subscribers = _streamManager.GetSubscribers(publishStreamContext.StreamPath);
            await BroadcastAudioMessageToSubscribersAsync(chunkStreamContext, clientContext, publishStreamContext, isSkippable, payloadBuffer, subscribers);
        }

        private async ValueTask BroadcastAudioMessageToSubscribersAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            bool isSkippable,
            IDataBuffer payloadBuffer,
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

        private (AudioCodec, AACPacketType?) ParseAudioMessageProperties(IDataBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var audioCodec = (AudioCodec)(firstByte >> 4);

            if (audioCodec is AudioCodec.AAC or AudioCodec.Opus)
            {
                var aacPackageType = (AACPacketType)payloadBuffer.ReadByte();
                return (audioCodec, aacPackageType);
            }

            return (audioCodec, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSkippable(AACPacketType? aacPacketType)
        {
            return aacPacketType != AACPacketType.SequenceHeader;
        }

        private async ValueTask HandleAudioPacketCachingAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            AACPacketType? aacPacketType,
            IDataBuffer payloadBuffer)
        {
            if (!aacPacketType.HasValue)
                return;

            if (aacPacketType == AACPacketType.SequenceHeader)
            {
                await _mediaMessageCacher.CacheSequenceHeaderAsync(publishStreamContext, MediaType.Audio, payloadBuffer);
            }
            else if (publishStreamContext.GroupOfPicturesCacheActivated)
            {
                await _mediaMessageCacher.CachePictureAsync(publishStreamContext, MediaType.Audio, payloadBuffer, chunkStreamContext.MessageHeader.Timestamp);
            }
        }
    }
}
