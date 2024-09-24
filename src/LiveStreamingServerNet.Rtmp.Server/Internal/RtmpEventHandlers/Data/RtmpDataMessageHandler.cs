﻿using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Data
{
    [RtmpMessageType(RtmpMessageType.DataMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.DataMessageAmf3)]
    internal class RtmpDataMessageHandler : IRtmpMessageHandler<IRtmpClientSessionContext>
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly ILogger<RtmpDataMessageHandler> _logger;

        public RtmpDataMessageHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpServerStreamEventDispatcher eventDispatcher,
            ILogger<RtmpDataMessageHandler> logger)
        {
            _streamManager = streamManager;
            _mediaMessageCacher = mediaMessageCacher;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
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
            IRtmpClientSessionContext clientContext,
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
            IRtmpClientSessionContext clientContext,
            IRtmpChunkStreamContext chunkStreamContext,
            IReadOnlyDictionary<string, object> metaData)
        {
            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var publishStreamContext = clientContext.GetStream(streamId)?.PublishContext;

            if (publishStreamContext == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.Id);
                return false;
            }

            CacheStreamMetaData(metaData, publishStreamContext);

            BroadcastMetaDataToSubscribers(chunkStreamContext, publishStreamContext);

            await _eventDispatcher.RtmpStreamMetaDataReceivedAsync(clientContext, publishStreamContext.StreamPath, metaData);

            return true;
        }

        private static void CacheStreamMetaData(IReadOnlyDictionary<string, object> metaData, IRtmpPublishStreamContext publishStreamContext)
        {
            publishStreamContext.StreamMetaData = new Dictionary<string, object>(metaData);
        }

        private void BroadcastMetaDataToSubscribers(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext)
        {
            var subscribeStreamContexts = _streamManager.GetSubscribeStreamContexts(publishStreamContext.StreamPath);

            _mediaMessageCacher.SendCachedStreamMetaDataMessage(
                subscribeStreamContexts,
                publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp);
        }
    }
}
