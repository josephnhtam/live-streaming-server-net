using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Logging;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal partial class RtmpMediaMessageManagerService : IRtmpMediaMessageManagerService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly IRtmpMediaMessageInterceptionService _interception;
        private readonly INetBufferPool _netBufferPool;
        private readonly MediaMessageConfiguration _config;
        private readonly ILogger _logger;

        public RtmpMediaMessageManagerService(
            IRtmpChunkMessageSenderService chunkMessageSender,
            IRtmpMediaMessageInterceptionService interception,
            INetBufferPool netBufferPool,
            IOptions<MediaMessageConfiguration> config,
            ILogger<RtmpMediaMessageManagerService> logger)
        {
            _chunkMessageSender = chunkMessageSender;
            _interception = interception;
            _netBufferPool = netBufferPool;
            _config = config.Value;
            _logger = logger;
        }

        public async ValueTask CacheSequenceHeaderAsync(
            IRtmpPublishStreamContext publishStreamContext,
            MediaType mediaType,
            INetBuffer payloadBuffer)
        {
            var sequenceHeader = payloadBuffer.MoveTo(0).ReadBytes(payloadBuffer.Size);
            payloadBuffer.MoveTo(0);

            await _interception.CacheSequenceHeaderAsync(publishStreamContext.StreamPath, mediaType, sequenceHeader);

            switch (mediaType)
            {
                case MediaType.Video:
                    publishStreamContext.VideoSequenceHeader = sequenceHeader;
                    break;
                case MediaType.Audio:
                    publishStreamContext.AudioSequenceHeader = sequenceHeader;
                    break;
            }
        }

        public async ValueTask CachePictureAsync(
            IRtmpPublishStreamContext publishStreamContext,
            MediaType mediaType,
            INetBuffer payloadBuffer,
            uint timestamp)
        {
            if (publishStreamContext.GroupOfPicturesCache.Size >= _config.MaxGroupOfPicturesCacheSize)
            {
                _logger.ReachedMaxGopCacheSize(publishStreamContext.StreamPath);
                await ClearGroupOfPicturesCacheAsync(publishStreamContext);
            }

            var rentedBuffer = new RentedBuffer(payloadBuffer.Size);
            payloadBuffer.MoveTo(0).ReadBytes(rentedBuffer.Buffer, 0, rentedBuffer.Size);
            payloadBuffer.MoveTo(0);

            await _interception.CachePictureAsync(publishStreamContext.StreamPath, mediaType, rentedBuffer, timestamp);
            publishStreamContext.GroupOfPicturesCache.Add(new PicturesCache(mediaType, timestamp, rentedBuffer));
        }

        public async ValueTask ClearGroupOfPicturesCacheAsync(IRtmpPublishStreamContext publishStreamContext)
        {
            await _interception.ClearGroupOfPicturesCacheAsync(publishStreamContext.StreamPath);
            publishStreamContext.GroupOfPicturesCache.Clear();
        }

        public void SendCachedHeaderMessages(
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint streamId)
        {
            var audioSequenceHeader = publishStreamContext.AudioSequenceHeader;
            if (audioSequenceHeader != null)
            {
                SendMediaPackage(clientContext, MediaType.Audio, audioSequenceHeader, audioSequenceHeader.Length, timestamp, streamId);
            }

            var videoSequenceHeader = publishStreamContext.VideoSequenceHeader;
            if (videoSequenceHeader != null)
            {
                SendMediaPackage(clientContext, MediaType.Video, videoSequenceHeader, videoSequenceHeader.Length, timestamp, streamId);
            }
        }

        public void SendCachedStreamMetaDataMessage(
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint streamId)
        {
            if (publishStreamContext.StreamMetaData == null)
                return;

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.DataMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(timestamp, RtmpMessageType.DataMessageAmf0, streamId);

            _chunkMessageSender.Send(clientContext, basicHeader, messageHeader, (netBuffer) =>
                netBuffer.WriteAmf(new List<object?>
                {
                    RtmpDataMessageConstants.OnMetaData,
                    publishStreamContext.StreamMetaData
                }, AmfEncodingType.Amf0)
            );
        }

        public void SendCachedStreamMetaDataMessage(
            IList<IRtmpClientContext> clientContexts,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint streamId)
        {
            if (publishStreamContext.StreamMetaData == null)
                return;

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.DataMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(timestamp, RtmpMessageType.DataMessageAmf0, streamId);

            _chunkMessageSender.Send(clientContexts, basicHeader, messageHeader, (netBuffer) =>
                netBuffer.WriteAmf(new List<object?>
                {
                    RtmpDataMessageConstants.OnMetaData,
                    publishStreamContext.StreamMetaData
                }, AmfEncodingType.Amf0)
            );
        }

        public void SendCachedGroupOfPictures(
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint streamId)
        {
            foreach (var picture in publishStreamContext.GroupOfPicturesCache.Get())
            {
                SendMediaPackage(clientContext, picture.Type, picture.Payload.Buffer, picture.Payload.Size, picture.Timestamp, streamId);
                picture.Payload.Unclaim();
            }
        }

        public async ValueTask EnqueueMediaMessageAsync(
            IRtmpPublishStreamContext publishStreamContext,
            IList<IRtmpClientContext> subscribers,
            MediaType mediaType,
            uint timestamp,
            bool isSkippable,
            Action<INetBuffer> payloadWriter)
        {
            subscribers = subscribers.Where(FilterSubscribers).ToList();

            using var netBuffer = _netBufferPool.Obtain();
            payloadWriter(netBuffer);

            var rentedBuffer = new RentedBuffer(netBuffer.Size, Math.Max(1, subscribers.Count));
            netBuffer.MoveTo(0).ReadBytes(rentedBuffer.Buffer, 0, rentedBuffer.Size);
            netBuffer.MoveTo(0);

            await _interception.ReceiveMediaMessageAsync(publishStreamContext.StreamPath, mediaType, rentedBuffer, timestamp, isSkippable);

            if (!subscribers.Any())
            {
                rentedBuffer.Unclaim();
                return;
            }

            var mediaPackage = new ClientMediaPackage(
                mediaType,
                timestamp,
                publishStreamContext.StreamId,
                rentedBuffer,
                isSkippable);

            foreach (var subscriber in subscribers)
            {
                var mediaContext = GetMediaContext(subscriber);
                if (mediaContext == null || !mediaContext.AddPackage(ref mediaPackage))
                    rentedBuffer.Unclaim();
            }

            bool FilterSubscribers(IRtmpClientContext subscriber)
            {
                var subscriptionContext = subscriber.StreamSubscriptionContext;

                if (subscriptionContext == null)
                    return false;

                switch (mediaType)
                {
                    case MediaType.Audio:
                        if (!subscriptionContext.IsReceivingAudio)
                            return false;
                        break;
                    case MediaType.Video:
                        if (!subscriptionContext.IsReceivingVideo)
                            return false;
                        break;
                }

                return true;
            }
        }

        private void SendMediaPackage(
            IRtmpClientContext clientContext,
            MediaType type,
            byte[] payloadBuffer,
            int payloadSize,
            uint timestamp,
            uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(
                    0,
                    type == MediaType.Video ?
                    RtmpConstants.VideoMessageChunkStreamId :
                    RtmpConstants.AudioMessageChunkStreamId);

            var messageHeader = new RtmpChunkMessageHeaderType0(
                timestamp,
                type == MediaType.Video ?
                RtmpMessageType.VideoMessage :
                RtmpMessageType.AudioMessage,
                streamId);

            _chunkMessageSender.Send(clientContext, basicHeader, messageHeader, (netBuffer) => netBuffer.Write(payloadBuffer, 0, payloadSize));
        }

        private async Task SendMediaPackageAsync(
            IRtmpClientContext clientContext,
            MediaType type,
            byte[] payloadBuffer,
            int payloadSize,
            uint timestamp,
            uint streamId,
            CancellationToken cancellation)
        {
            var basicHeader = new RtmpChunkBasicHeader(
                    0,
                    type == MediaType.Video ?
                    RtmpConstants.VideoMessageChunkStreamId :
                    RtmpConstants.AudioMessageChunkStreamId);

            var messageHeader = new RtmpChunkMessageHeaderType0(
                timestamp,
                type == MediaType.Video ?
                RtmpMessageType.VideoMessage :
                RtmpMessageType.AudioMessage,
                streamId);

            await _chunkMessageSender
                .SendAsync(clientContext, basicHeader, messageHeader,
                    (netBuffer) => netBuffer.Write(payloadBuffer, 0, payloadSize))
                .WithCancellation(cancellation);
        }
    }
}
