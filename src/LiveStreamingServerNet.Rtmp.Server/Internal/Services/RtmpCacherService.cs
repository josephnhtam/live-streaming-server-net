using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpCacherService : IRtmpCacherService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly IRtmpMediaCachingInterceptionService _interception;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly MediaStreamingConfiguration _config;
        private readonly ILogger _logger;

        public RtmpCacherService(
            IRtmpChunkMessageSenderService chunkMessageSender,
            IRtmpMediaCachingInterceptionService interception,
            IRtmpServerStreamEventDispatcher eventDispatcher,
            IOptions<MediaStreamingConfiguration> config,
            ILogger<RtmpCacherService> logger)
        {
            _chunkMessageSender = chunkMessageSender;
            _interception = interception;
            _eventDispatcher = eventDispatcher;
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

            await _interception.CacheSequenceHeaderAsync(publishStreamContext, mediaType, sequenceHeader);

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

        public async ValueTask CacheStreamMetaDataAsync(IRtmpPublishStreamContext publishStreamContext, IReadOnlyDictionary<string, object> metaData)
        {
            publishStreamContext.StreamMetaData = new Dictionary<string, object>(metaData);
            await _eventDispatcher.RtmpStreamMetaDataReceivedAsync(publishStreamContext);
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

            await _interception.CachePictureAsync(publishStreamContext, mediaType, payloadBuffer, timestamp);
            publishStreamContext.GroupOfPicturesCache.Add(new PictureCacheInfo(mediaType, timestamp), payloadBuffer);
        }

        public async ValueTask ClearGroupOfPicturesCacheAsync(IRtmpPublishStreamContext publishStreamContext)
        {
            await _interception.ClearGroupOfPicturesCacheAsync(publishStreamContext);
            publishStreamContext.GroupOfPicturesCache.Clear();
        }

        public void SendCachedHeaderMessages(
            IRtmpSubscribeStreamContext subscribeStreamContext,
            IRtmpPublishStreamContext publishStreamContext)
        {
            var audioSequenceHeader = publishStreamContext.AudioSequenceHeader;
            if (audioSequenceHeader != null)
            {
                SendMediaPacket(subscribeStreamContext, MediaType.Audio, audioSequenceHeader, audioSequenceHeader.Length, publishStreamContext.TimestampOffset, true);
            }

            var videoSequenceHeader = publishStreamContext.VideoSequenceHeader;
            if (videoSequenceHeader != null)
            {
                SendMediaPacket(subscribeStreamContext, MediaType.Video, videoSequenceHeader, videoSequenceHeader.Length, publishStreamContext.TimestampOffset, true);
            }
        }

        public void SendCachedStreamMetaDataMessage(
            IRtmpSubscribeStreamContext subscribeStreamContext,
            IRtmpPublishStreamContext publishStreamContext)
        {
            if (publishStreamContext.StreamMetaData == null)
                return;

            var basicHeader = new RtmpChunkBasicHeader(0, subscribeStreamContext.DataChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(publishStreamContext.TimestampOffset, RtmpMessageType.DataMessageAmf0, subscribeStreamContext.StreamContext.StreamId);

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
            IRtmpPublishStreamContext publishStreamContext)
        {
            if (publishStreamContext.StreamMetaData == null)
                return;

            foreach (var group in subscribeStreamContexts.GroupBy(x => (x.StreamContext.StreamId, x.DataChunkStreamId)))
            {
                var streamId = group.Key.StreamId;
                var chunkStreamId = group.Key.DataChunkStreamId;
                var clientContexts = group.Select(x => x.StreamContext.ClientContext).ToList();

                var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(publishStreamContext.TimestampOffset, RtmpMessageType.DataMessageAmf0, streamId);

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
            var pictures = publishStreamContext.GroupOfPicturesCache.Get();

            try
            {
                foreach (var picture in pictures)
                {
                    SendMediaPacket(
                        subscribeStreamContext,
                        picture.Type,
                        picture.Payload.Buffer,
                        picture.Payload.Size,
                        picture.Timestamp + publishStreamContext.TimestampOffset,
                        false);
                }
            }
            finally
            {
                foreach (var picture in pictures)
                    picture.Payload.Unclaim();
            }
        }

        private void SendMediaPacket(
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
                type == MediaType.Audio ?
                subscribeStreamContext.AudioChunkStreamId :
                subscribeStreamContext.VideoChunkStreamId);

            var messageHeader = new RtmpChunkMessageHeaderType0(
                timestamp,
                type == MediaType.Audio ?
                RtmpMessageType.AudioMessage :
                RtmpMessageType.VideoMessage,
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
