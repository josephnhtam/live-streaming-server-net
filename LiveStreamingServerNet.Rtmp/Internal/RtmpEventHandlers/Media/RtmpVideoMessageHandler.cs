using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.MessageDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Media
{
    [RtmpMessageType(RtmpMessageType.VideoMessage)]
    internal class RtmpVideoMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger _logger;

        public RtmpVideoMessageHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageManagerService mediaMessageManager,
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpVideoMessageHandler> logger)
        {
            _streamManager = streamManager;
            _mediaMessageManager = mediaMessageManager;
            _config = config.Value;
            _logger = logger;
        }

        public Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var publishStreamContext = clientContext.PublishStreamContext ??
                throw new InvalidOperationException("Stream is not yet published.");

            var hasHeader = CacheVideoSequence(chunkStreamContext, publishStreamContext, payloadBuffer);
            BroacastVideoMessageToSubscribers(chunkStreamContext, publishStreamContext, payloadBuffer, hasHeader);
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
            IList<IRtmpClientContext> subscribers)
        {
            _mediaMessageManager.EnqueueMediaMessage(
                subscribers,
                MediaType.Video,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId,
                isSkippable,
                payloadBuffer.CopyAllTo);
        }

        private bool CacheVideoSequence(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var frameType = (VideoFrameType)(firstByte >> 4);
            var codecId = (VideoCodecId)(firstByte & 0x0f);

            if (codecId == VideoCodecId.AVC)
            {
                var avcPackageType = (AVCPacketType)payloadBuffer.ReadByte();

                if (_config.EnableGopCaching && frameType == VideoFrameType.KeyFrame)
                {
                    _mediaMessageManager.ClearGroupOfPicturesCache(publishStreamContext);
                }

                if (frameType == VideoFrameType.KeyFrame && avcPackageType == AVCPacketType.SequenceHeader)
                {
                    publishStreamContext.VideoSequenceHeader = payloadBuffer.MoveTo(0).ReadBytes(payloadBuffer.Size);
                    payloadBuffer.MoveTo(0);
                    return true;
                }

                if (_config.EnableGopCaching && avcPackageType == AVCPacketType.NALU)
                {
                    _mediaMessageManager.CachePictures(publishStreamContext, MediaType.Video, payloadBuffer, chunkStreamContext.MessageHeader.Timestamp);
                }
            }

            payloadBuffer.MoveTo(0);

            return false;
        }
    }
}
