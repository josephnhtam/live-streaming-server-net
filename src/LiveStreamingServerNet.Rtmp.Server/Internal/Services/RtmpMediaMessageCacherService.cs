using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpMediaMessageCacherService : IRtmpMediaMessageCacherService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly IRtmpMediaCachingInterceptionService _interception;
        private readonly MediaMessageConfiguration _config;
        private readonly ILogger _logger;

        public RtmpMediaMessageCacherService(
            IRtmpChunkMessageSenderService chunkMessageSender,
            IRtmpMediaCachingInterceptionService interception,
            IOptions<MediaMessageConfiguration> config,
            ILogger<RtmpMediaMessageCacherService> logger)
        {
            _chunkMessageSender = chunkMessageSender;
            _interception = interception;
            _config = config.Value;
            _logger = logger;
        }

        public async ValueTask CacheSequenceHeaderAsync(
            IRtmpPublishStreamContext publishStreamContext,
            MediaType mediaType,
            IDataBuffer payloadBuffer)
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
            IDataBuffer payloadBuffer,
            uint timestamp)
        {
            if (publishStreamContext.GroupOfPicturesCache.Size >= _config.MaxGroupOfPicturesCacheSize)
            {
                _logger.ReachedMaxGopCacheSize(publishStreamContext.StreamPath);
                await ClearGroupOfPicturesCacheAsync(publishStreamContext);
            }

            await _interception.CachePictureAsync(publishStreamContext.StreamPath, mediaType, payloadBuffer, timestamp);
            publishStreamContext.GroupOfPicturesCache.Add(new PictureCacheInfo(mediaType, timestamp), payloadBuffer);
        }

        public async ValueTask ClearGroupOfPicturesCacheAsync(IRtmpPublishStreamContext publishStreamContext)
        {
            await _interception.ClearGroupOfPicturesCacheAsync(publishStreamContext.StreamPath);
            publishStreamContext.GroupOfPicturesCache.Clear();
        }

        public void SendCachedHeaderMessages(
            IRtmpSubscribeStreamContext subscribeStreamContext,
            IRtmpPublishStreamContext publishStreamContext)
        {
            var audioSequenceHeader = publishStreamContext.AudioSequenceHeader;
            if (audioSequenceHeader != null)
            {
                SendMediaPackage(subscribeStreamContext, MediaType.Audio, audioSequenceHeader, audioSequenceHeader.Length, 0, true);
            }

            var videoSequenceHeader = publishStreamContext.VideoSequenceHeader;
            if (videoSequenceHeader != null)
            {
                SendMediaPackage(subscribeStreamContext, MediaType.Video, videoSequenceHeader, videoSequenceHeader.Length, 0, true);
            }
        }

        public void SendCachedStreamMetaDataMessage(
            IRtmpSubscribeStreamContext subscribeStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp)
        {
            if (publishStreamContext.StreamMetaData == null)
                return;

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.DataMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(timestamp, RtmpMessageType.DataMessageAmf0, subscribeStreamContext.StreamContext.StreamId);

            _chunkMessageSender.Send(subscribeStreamContext.StreamContext.ClientContext, basicHeader, messageHeader, (dataBuffer) =>
                dataBuffer.WriteAmf(new List<object?>
                {
                    RtmpDataMessageConstants.OnMetaData,
                    publishStreamContext.StreamMetaData
                }, AmfEncodingType.Amf0)
            );
        }

        public void SendCachedStreamMetaDataMessage(
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp)
        {
            if (publishStreamContext.StreamMetaData == null)
                return;

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.DataMessageChunkStreamId);

            foreach (var group in subscribeStreamContexts.GroupBy(x => x.StreamContext.StreamId))
            {
                var streamId = group.Key;
                var clientContexts = group.Select(x => x.StreamContext.ClientContext).ToList();

                var messageHeader = new RtmpChunkMessageHeaderType0(timestamp, RtmpMessageType.DataMessageAmf0, streamId);

                _chunkMessageSender.Send(clientContexts, basicHeader, messageHeader, (dataBuffer) =>
                    dataBuffer.WriteAmf(new List<object?>
                    {
                        RtmpDataMessageConstants.OnMetaData,
                        publishStreamContext.StreamMetaData
                    }, AmfEncodingType.Amf0)
                );
            }
        }

        public void SendCachedGroupOfPictures(
            IRtmpSubscribeStreamContext subscribeStreamContext,
            IRtmpPublishStreamContext publishStreamContext)
        {
            foreach (var picture in publishStreamContext.GroupOfPicturesCache.Get())
            {
                SendMediaPackage(
                    subscribeStreamContext,
                    picture.Type,
                    picture.Payload.Buffer,
                    picture.Payload.Size,
                    picture.Timestamp,
                    false);

                picture.Payload.Unclaim();
            }
        }

        private void SendMediaPackage(
            IRtmpSubscribeStreamContext subscribeStreamContext,
            MediaType type,
            byte[] payloadBuffer,
            int payloadSize,
            uint timestamp,
            bool isHeader)
        {
            if (!subscribeStreamContext.UpdateTimestamp(timestamp, type) && !isHeader)
                return;

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
                subscribeStreamContext.StreamContext.StreamId);

            _chunkMessageSender.Send(
                subscribeStreamContext.StreamContext.ClientContext,
                basicHeader,
                messageHeader,
                (dataBuffer) => dataBuffer.Write(payloadBuffer, 0, payloadSize));
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
