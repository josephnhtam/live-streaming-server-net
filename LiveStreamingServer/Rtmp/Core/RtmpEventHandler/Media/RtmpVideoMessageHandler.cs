using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpHeaders;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Media
{
    [RtmpMessageType(RtmpMessageType.VideoMessage)]
    public class RtmpVideoMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpServerContext _serverContext;
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly ILogger _logger;

        public RtmpVideoMessageHandler(IRtmpServerContext serverContext, IRtmpChunkMessageSenderService chunkMessageSender, ILogger<RtmpVideoMessageHandler> logger)
        {
            _serverContext = serverContext;
            _chunkMessageSender = chunkMessageSender;
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
            BroacastVideoMessageToSubscribers(peerContext, chunkStreamContext, publishStreamContext, payloadBuffer, hasSequenceHeader);
            return Task.FromResult(true);
        }

        private void BroacastVideoMessageToSubscribers(
            IRtmpClientPeerContext peerContext,
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
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.VideoMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(chunkStreamContext.MessageHeader.Timestamp,
                RtmpMessageType.VideoMessage, chunkStreamContext.MessageHeader.MessageStreamId);

            _chunkMessageSender.Send(subscribers, basicHeader, messageHeader, payloadBuffer.Flush);
        }

        private static bool CacheVideoSequenceHeaderIfNeeded(
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var frameType = (VideoFrameType)(firstByte >> 4);
            var codecId = (VideoCodecId)(firstByte & 0x0f);

            payloadBuffer.MoveTo(0);

            if (frameType == VideoFrameType.KeyFrame && codecId == VideoCodecId.AVC)
            {
                var avcPackageType = (AVCPacketType)payloadBuffer.ReadByte();
                if (avcPackageType == AVCPacketType.SequenceHeader)
                {
                    publishStreamContext.VideoSequenceHeader = new byte[payloadBuffer.Size];
                    payloadBuffer.ReadBytes(payloadBuffer.Size);
                    payloadBuffer.MoveTo(0);
                    return true;
                }
            }

            return false;
        }
    }
}
