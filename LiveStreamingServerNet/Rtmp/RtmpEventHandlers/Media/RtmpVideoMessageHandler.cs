using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Media
{
    [RtmpMessageType(RtmpMessageType.VideoMessage)]
    public class RtmpVideoMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;
        private readonly ILogger _logger;

        public RtmpVideoMessageHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageManagerService mediaMessageManager,
            ILogger<RtmpVideoMessageHandler> logger)
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
                using var subscribers = _streamManager.GetSubscribersLocked(publishStreamContext.StreamPath);
                BroacastVideoMessageToSubscribers(chunkStreamContext, false, payloadBuffer, subscribers.Value);
            }
            else
            {
                var subscribers = _streamManager.GetSubscribers(publishStreamContext.StreamPath);
                BroacastVideoMessageToSubscribers(chunkStreamContext, true, payloadBuffer, subscribers);
            }
        }

        private void BroacastVideoMessageToSubscribers(
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            INetBuffer payloadBuffer,
            IList<IRtmpClientPeerContext> subscribers)
        {
            _mediaMessageManager.SendVideoMessage(subscribers, chunkStreamContext, isSkippable, payloadBuffer.CopyAllTo);
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
                    publishStreamContext.VideoSequenceHeader = payloadBuffer.MoveTo(0).ReadBytes(payloadBuffer.Size);
                    payloadBuffer.MoveTo(0);
                    return true;
                }
            }

            payloadBuffer.MoveTo(0);

            return false;
        }
    }
}
