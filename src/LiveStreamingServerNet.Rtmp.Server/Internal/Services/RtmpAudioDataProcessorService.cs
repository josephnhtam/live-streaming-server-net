using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Filtering.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpAudioDataProcessorService : IRtmpAudioDataProcessorService
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpMediaMessageBroadcasterService _mediaMessageBroadcaster;
        private readonly ILogger _logger;

        private readonly IFilter<AudioCodec>? _audioCodecFilter;

        public RtmpAudioDataProcessorService(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpMediaMessageBroadcasterService mediaMessageBroadcaster,
            ILogger<RtmpAudioDataProcessorService> logger,
            IFilter<AudioCodec>? audioCodecFilter = null)
        {
            _streamManager = streamManager;
            _mediaMessageCacher = mediaMessageCacher;
            _mediaMessageBroadcaster = mediaMessageBroadcaster;
            _logger = logger;

            _audioCodecFilter = audioCodecFilter;
        }

        public async ValueTask<bool> ProcessAudioDataAsync(IRtmpPublishStreamContext publishStreamContext, uint timestamp, IDataBuffer payloadBuffer)
        {
            var (audioCodec, aacPacketType) = ParseAudioMessageProperties(payloadBuffer);

            if (!IsAudioCodecAllowed(publishStreamContext, audioCodec)) return false;

            await HandleAudioPacketCachingAsync(
                publishStreamContext,
                timestamp,
                aacPacketType,
                payloadBuffer.MoveTo(0));

            await BroadcastAudioMessageToSubscribersAsync(
                publishStreamContext,
                timestamp,
                IsSkippable(aacPacketType),
                payloadBuffer.MoveTo(0));

            return true;
        }

        private async ValueTask BroadcastAudioMessageToSubscribersAsync(
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            bool isSkippable,
            IDataBuffer payloadBuffer)
        {
            var subscribeStreamContexts = _streamManager.GetSubscribeStreamContexts(publishStreamContext.StreamPath);
            await BroadcastAudioMessageToSubscribersAsync(publishStreamContext, timestamp, isSkippable, payloadBuffer, subscribeStreamContexts);
        }

        private async ValueTask BroadcastAudioMessageToSubscribersAsync(
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            bool isSkippable,
            IDataBuffer payloadBuffer,
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            publishStreamContext.UpdateTimestamp(timestamp, MediaType.Audio);

            await _mediaMessageBroadcaster.BroadcastMediaMessageAsync(
                publishStreamContext,
                subscribeStreamContexts,
                MediaType.Audio,
                timestamp,
                isSkippable,
                payloadBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsAudioCodecAllowed(IRtmpPublishStreamContext publishStreamContext, AudioCodec audioCodec)
        {
            if (_audioCodecFilter != null && !_audioCodecFilter.IsAllowed(audioCodec))
            {
                _logger.AudioCodecNotAllowed(publishStreamContext.StreamPath, audioCodec);
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
                var aacPacketType = (AACPacketType)payloadBuffer.ReadByte();
                return (audioCodec, aacPacketType);
            }

            return (audioCodec, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSkippable(AACPacketType? aacPacketType)
        {
            return aacPacketType != AACPacketType.SequenceHeader;
        }

        private async ValueTask HandleAudioPacketCachingAsync(
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
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
                await _mediaMessageCacher.CachePictureAsync(publishStreamContext, MediaType.Audio, payloadBuffer, timestamp);
            }
        }
    }
}
