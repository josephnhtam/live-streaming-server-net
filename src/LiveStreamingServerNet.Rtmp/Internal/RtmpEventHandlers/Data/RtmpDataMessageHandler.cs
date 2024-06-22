using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Data
{
    [RtmpMessageType(RtmpMessageType.DataMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.DataMessageAmf3)]
    internal class RtmpDataMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly ILogger<RtmpDataMessageHandler> _logger;

        public RtmpDataMessageHandler(
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpServerStreamEventDispatcher eventDispatcher,
            ILogger<RtmpDataMessageHandler> logger)
        {
            _mediaMessageCacher = mediaMessageCacher;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var amfData = chunkStreamContext.MessageHeader.MessageTypeId switch
            {
                RtmpMessageType.DataMessageAmf0 => payloadBuffer.ReadAmf(payloadBuffer.Size, 3, AmfEncodingType.Amf0),
                RtmpMessageType.DataMessageAmf3 => payloadBuffer.ReadAmf(payloadBuffer.Size, 3, AmfEncodingType.Amf3),
                _ => throw new ArgumentOutOfRangeException()
            };

            var commandName = (string)amfData[0];

            return commandName switch
            {
                RtmpDataMessageConstants.SetDataFrame => await HandleSetDataFrameAsync(clientContext, chunkStreamContext, amfData),
                _ => true
            };
        }

        private async ValueTask<bool> HandleSetDataFrameAsync(
            IRtmpClientContext clientContext,
            IRtmpChunkStreamContext chunkStreamContext,
            object[] amfData)
        {
            var eventName = amfData[1] as string;
            switch (eventName)
            {
                case RtmpDataMessageConstants.OnMetaData:
                    var metaData = amfData[2] as Dictionary<string, object>;
                    return metaData != null ? await HandleOnMetaDataAsync(clientContext, chunkStreamContext, metaData.AsReadOnly()) : true;
                default:
                    return true;
            }
        }

        private async ValueTask<bool> HandleOnMetaDataAsync(
            IRtmpClientContext clientContext,
            IRtmpChunkStreamContext chunkStreamContext,
            IReadOnlyDictionary<string, object> metaData)
        {
            var publishStreamContext = clientContext.PublishStreamContext;

            if (publishStreamContext == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.ClientId);
                return false;
            }

            CacheStreamMetaData(metaData, publishStreamContext);

            BroadcastMetaDataToSubscribers(clientContext, chunkStreamContext, publishStreamContext);

            await _eventDispatcher.RtmpStreamMetaDataReceivedAsync(clientContext, clientContext.PublishStreamContext!.StreamPath, metaData);

            return true;
        }

        private static void CacheStreamMetaData(IReadOnlyDictionary<string, object> metaData, IRtmpPublishStreamContext publishStreamContext)
        {
            publishStreamContext.StreamMetaData = new Dictionary<string, object>(metaData);
        }

        private void BroadcastMetaDataToSubscribers(
            IRtmpClientContext clientContext,
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext)
        {
            _mediaMessageCacher.SendCachedStreamMetaDataMessage(
                clientContext,
                publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId);
        }
    }
}
