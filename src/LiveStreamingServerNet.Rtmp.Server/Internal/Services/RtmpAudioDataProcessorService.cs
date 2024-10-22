using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Filtering.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Utilities.Containers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpAudioDataProcessorService : IRtmpAudioDataProcessorService
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCacherService _cacher;
        private readonly IRtmpMediaMessageBroadcasterService _mediaMessageBroadcaster;
        private readonly ILogger _logger;

        private readonly IFilter<AudioCodec>? _audioCodecFilter;

        public RtmpAudioDataProcessorService(
            IRtmpStreamManagerService streamManager,
            IRtmpCacherService cacher,
            IRtmpMediaMessageBroadcasterService mediaMessageBroadcaster,
            ILogger<RtmpAudioDataProcessorService> logger,
            IFilter<AudioCodec>? audioCodecFilter = null)
        {
            _streamManager = streamManager;
            _cacher = cacher;
            _mediaMessageBroadcaster = mediaMessageBroadcaster;
            _logger = logger;

            _audioCodecFilter = audioCodecFilter;
        }

        public async ValueTask<bool> ProcessAudioDataAsync(IRtmpPublishStreamContext publishStreamContext, uint timestamp, IDataBuffer payloadBuffer)
        {
            var (audioCodec, _, _, _, aacPacketType) = FlvParser.ParseAudioTagHeader(payloadBuffer.AsSpan());

            if (!IsAudioCodecAllowed(publishStreamContext, audioCodec)) return false;

            publishStreamContext.UpdateTimestamp(timestamp, MediaType.Audio);

            await HandleAudioPacketCachingAsync(
                publishStreamContext,
                aacPacketType,
                payloadBuffer.MoveTo(0));

            await BroadcastAudioMessageToSubscribersAsync(
                publishStreamContext,
                IsSkippable(aacPacketType),
                payloadBuffer.MoveTo(0));

            return true;
        }

        private async ValueTask BroadcastAudioMessageToSubscribersAsync(
            IRtmpPublishStreamContext publishStreamContext,
            bool isSkippable,
            IDataBuffer payloadBuffer)
        {
            var subscribeStreamContexts = _streamManager.GetSubscribeStreamContexts(publishStreamContext.StreamPath);

            await _mediaMessageBroadcaster.BroadcastMediaMessageAsync(
                publishStreamContext,
                subscribeStreamContexts,
                MediaType.Audio,
                publishStreamContext.AudioTimestamp,
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSkippable(AACPacketType? aacPacketType)
        {
            return aacPacketType != AACPacketType.SequenceHeader;
        }

        private async ValueTask HandleAudioPacketCachingAsync(
            IRtmpPublishStreamContext publishStreamContext,
            AACPacketType? aacPacketType,
            IDataBuffer payloadBuffer)
        {
            if (!aacPacketType.HasValue)
                return;

            if (aacPacketType == AACPacketType.SequenceHeader)
            {
                await _cacher.CacheSequenceHeaderAsync(publishStreamContext, MediaType.Audio, payloadBuffer);
            }
            else if (publishStreamContext.GroupOfPicturesCacheActivated)
            {
                await _cacher.CachePictureAsync(publishStreamContext, MediaType.Audio, payloadBuffer, publishStreamContext.AudioTimestamp);
            }
        }
    }
}
