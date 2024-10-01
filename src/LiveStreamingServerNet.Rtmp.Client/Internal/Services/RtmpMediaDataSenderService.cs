using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services
{
    internal class RtmpMediaDataSenderService : IRtmpMediaDataSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;

        public RtmpMediaDataSenderService(IRtmpChunkMessageSenderService chunkMessageSender)
        {
            _chunkMessageSender = chunkMessageSender;
        }

        public ValueTask SendMetaDataAsync(IRtmpPublishStreamContext publishStreamContext, IReadOnlyDictionary<string, object> metaData)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, publishStreamContext.DataChunkStreamId);

            var messageHeader = new RtmpChunkMessageHeaderType0(
                timestamp: 0,
                RtmpMessageType.DataMessageAmf0,
                publishStreamContext.StreamContext.StreamId);

            return _chunkMessageSender.SendAsync(basicHeader, messageHeader, buffer =>
                buffer.WriteAmf(new List<object?> {
                    RtmpDataMessageConstants.SetDataFrame,
                    RtmpDataMessageConstants.OnMetaData,
                    metaData
                }, AmfEncodingType.Amf0));
        }

        public ValueTask SendAudioDataAsync(IRtmpPublishStreamContext publishStreamContext, IRentedBuffer payload, uint timestamp)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, publishStreamContext.AudioChunkStreamId);

            var messageHeader = new RtmpChunkMessageHeaderType0(
                timestamp,
                payload.Size,
                RtmpMessageType.AudioMessage,
                publishStreamContext.StreamContext.StreamId);

            return _chunkMessageSender.SendAsync(basicHeader, messageHeader, payload);
        }

        public ValueTask SendVideoDataAsync(IRtmpPublishStreamContext publishStreamContext, IRentedBuffer payload, uint timestamp)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, publishStreamContext.VideoChunkStreamId);

            var messageHeader = new RtmpChunkMessageHeaderType0(
                timestamp,
                payload.Size,
                RtmpMessageType.VideoMessage,
                publishStreamContext.StreamContext.StreamId);

            return _chunkMessageSender.SendAsync(basicHeader, messageHeader, payload);
        }
    }
}
