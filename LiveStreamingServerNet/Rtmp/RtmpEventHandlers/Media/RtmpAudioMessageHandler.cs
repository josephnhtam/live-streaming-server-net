using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Media
{
    [RtmpMessageType(RtmpMessageType.AudioMessage)]
    public class RtmpAudioMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;
        private readonly ILogger _logger;

        public RtmpAudioMessageHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageManagerService mediaMessageManager,
            ILogger<RtmpAudioMessageHandler> logger)
        {
            _streamManager = streamManager;
            _mediaMessageManager = mediaMessageManager;
            _logger = logger;
        }

        public Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var publishStreamContext = peerContext.PublishStreamContext ??
                throw new InvalidOperationException("Stream is not created yet.");

            var hasSequenceHeader = CacheAudioSequenceHeaderIfNeeded(publishStreamContext, payloadBuffer);
            BroacastAudioMessageToSubscribers(chunkStreamContext, publishStreamContext, payloadBuffer, hasSequenceHeader);
            return Task.FromResult(true);
        }

        private void BroacastAudioMessageToSubscribers(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer,
            bool hasSequenceHeader)
        {
            if (hasSequenceHeader)
            {
                using var subscribers = _streamManager.GetSubscribersLocked(publishStreamContext.StreamPath);
                BroacastAudioMessageToSubscribers(chunkStreamContext, false, payloadBuffer, subscribers.Value);
            }
            else
            {
                var subscribers = _streamManager.GetSubscribers(publishStreamContext.StreamPath);
                BroacastAudioMessageToSubscribers(chunkStreamContext, true, payloadBuffer, subscribers);
            }
        }

        private void BroacastAudioMessageToSubscribers(
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            INetBuffer payloadBuffer,
            IList<IRtmpClientPeerContext> subscribers)
        {
            _mediaMessageManager.EnqueueAudioMessage(subscribers, chunkStreamContext, isSkippable, payloadBuffer.Flush);
        }

        private static bool CacheAudioSequenceHeaderIfNeeded(
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var soundFormat = (AudioSoundFormat)(firstByte >> 4);

            if (soundFormat == AudioSoundFormat.AAC)
            {
                var aacPackageType = (AACPacketType)payloadBuffer.ReadByte();
                if (aacPackageType == AACPacketType.SequenceHeader)
                {
                    publishStreamContext.AudioSequenceHeader = payloadBuffer.MoveTo(0).ReadBytes(payloadBuffer.Size);
                    payloadBuffer.MoveTo(0);
                    return true;
                }
            }

            payloadBuffer.MoveTo(0);

            return false;
        }
    }
}
