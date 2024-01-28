using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Data
{
    [RtmpMessageType(RtmpMessageType.DataMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.DataMessageAmf3)]
    internal class RtmpDataMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;

        public RtmpDataMessageHandler(IRtmpMediaMessageManagerService mediaMessageManager, IRtmpServerStreamEventDispatcher eventDispatcher)
        {
            _mediaMessageManager = mediaMessageManager;
            _eventDispatcher = eventDispatcher;
        }

        public async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            INetBuffer payloadBuffer,
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
                    var metaData = amfData[2] as IDictionary<string, object>;
                    return metaData != null ? await HandleOnMetaDataAsync(clientContext, chunkStreamContext, metaData) : true;
                default:
                    return true;
            }
        }

        private ValueTask<bool> HandleOnMetaDataAsync(
            IRtmpClientContext clientContext,
            IRtmpChunkStreamContext chunkStreamContext,
            IDictionary<string, object> metaData)
        {
            var publishStreamContext = clientContext.PublishStreamContext
                ?? throw new InvalidOperationException("Stream is not yet created.");

            CacheStreamMetaData(metaData, publishStreamContext);

            BroadcastMetaDataToSubscribers(clientContext, chunkStreamContext, publishStreamContext);

            _eventDispatcher.RtmpStreamMetaDataReceived(clientContext, clientContext.PublishStreamContext!.StreamPath, metaData.AsReadOnly());

            return ValueTask.FromResult(true);
        }

        private static void CacheStreamMetaData(IDictionary<string, object> metaData, IRtmpPublishStreamContext publishStreamContext)
        {
            publishStreamContext.StreamMetaData = metaData;
        }

        private void BroadcastMetaDataToSubscribers(
            IRtmpClientContext clientContext,
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext)
        {
            _mediaMessageManager.SendCachedStreamMetaDataMessage(
                clientContext,
                publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId);
        }
    }
}
