using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Media
{
    [RtmpMessageType(RtmpMessageType.AudioMessage)]
    public class RtmpAudioMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpServerContext _serverContext;
        private readonly IRtmpMediaMessageSenderService _mediaMessageSender;
        private readonly ILogger _logger;

        public RtmpAudioMessageHandler(IRtmpServerContext serverContext, IRtmpMediaMessageSenderService mediaMessageSender, ILogger<RtmpAudioMessageHandler> logger)
        {
            _serverContext = serverContext;
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
                using var subscribers = _serverContext.GetSubscribersLocked(publishStreamContext.StreamPath);
                BroacastAudioMessageToSubscribers(chunkStreamContext, payloadBuffer, subscribers.Value);
            }
            else
            {
                var subscribers = _serverContext.GetSubscribers(publishStreamContext.StreamPath);
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
                    publishStreamContext.VideoSequenceHeader = new byte[payloadBuffer.Size];
                    payloadBuffer.MoveTo(0).ReadBytes(payloadBuffer.Size);
                    payloadBuffer.MoveTo(0);
                    return true;
                }
            }

            payloadBuffer.MoveTo(0);

            return false;
        }
    }
}
