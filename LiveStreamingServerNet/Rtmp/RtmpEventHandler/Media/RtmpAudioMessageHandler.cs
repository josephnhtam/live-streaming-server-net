using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.Media
{
    [RtmpMessageType(RtmpMessageType.AudioMessage)]
    public class RtmpAudioMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageSenderService _mediaMessageSender;
        private readonly ILogger _logger;

        public RtmpAudioMessageHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageSenderService mediaMessageSender,
            ILogger<RtmpAudioMessageHandler> logger)
        {
            _streamManager = streamManager;
            _mediaMessageSender = mediaMessageSender;
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
                BroacastAudioMessageToSubscribers(chunkStreamContext, payloadBuffer, subscribers.Value);
            }
            else
            {
                var subscribers = _streamManager.GetSubscribers(publishStreamContext.StreamPath);
                BroacastAudioMessageToSubscribers(chunkStreamContext, payloadBuffer, subscribers);
            }
        }

        private void BroacastAudioMessageToSubscribers(
            IRtmpChunkStreamContext chunkStreamContext,
            INetBuffer payloadBuffer,
            IList<IRtmpClientPeerContext> subscribers)
        {
            _mediaMessageSender.SendAudioMessage(subscribers, chunkStreamContext, payloadBuffer.Flush);
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
