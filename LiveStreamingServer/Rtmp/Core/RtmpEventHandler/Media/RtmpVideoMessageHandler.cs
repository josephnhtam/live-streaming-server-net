using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Media
{
    [RtmpMessageType(RtmpMessageType.VideoMessage)]
    public class RtmpVideoMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpServerContext _serverContext;
        private readonly IRtmpMediaMessageSenderService _mediaMessageSender;
        private readonly ILogger _logger;

        public RtmpVideoMessageHandler(IRtmpServerContext serverContext, IRtmpMediaMessageSenderService mediaMessageSender, ILogger<RtmpVideoMessageHandler> logger)
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

            var hasSequenceHeader = CacheVideoSequenceHeaderIfNeeded(publishStreamContext, payloadBuffer);
            BroacastVideoMessageToSubscribers(chunkStreamContext, publishStreamContext, payloadBuffer, hasSequenceHeader);
            return Task.FromResult(true);
        }

        private void BroacastVideoMessageToSubscribers(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer,
            bool hasSequenceHeader)
        {
            if (hasSequenceHeader)
            {
                using var subscribers = _serverContext.GetSubscribersLocked(publishStreamContext.StreamPath);
                BroacastVideoMessageToSubscribers(chunkStreamContext, payloadBuffer, subscribers.Value);
            }
            else
            {
                var subscribers = _serverContext.GetSubscribers(publishStreamContext.StreamPath);
                BroacastVideoMessageToSubscribers(chunkStreamContext, payloadBuffer, subscribers);
            }
        }

        private void BroacastVideoMessageToSubscribers(
            IRtmpChunkStreamContext chunkStreamContext,
            INetBuffer payloadBuffer,
            IList<IRtmpClientPeerContext> subscribers)
        {
            _mediaMessageSender.SendVideoMessage(subscribers, chunkStreamContext, payloadBuffer.CopyAllTo);
        }

        private static bool CacheVideoSequenceHeaderIfNeeded(
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var frameType = (VideoFrameType)(firstByte >> 4);
            var codecId = (VideoCodecId)(firstByte & 0x0f);

            if (frameType == VideoFrameType.KeyFrame && codecId == VideoCodecId.AVC)
            {
                var avcPackageType = (AVCPacketType)payloadBuffer.ReadByte();
                if (avcPackageType == AVCPacketType.SequenceHeader)
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
