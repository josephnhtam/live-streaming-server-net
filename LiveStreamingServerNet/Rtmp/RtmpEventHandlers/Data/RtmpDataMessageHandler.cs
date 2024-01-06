using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Data
{
    [RtmpMessageType(RtmpMessageType.DataMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.DataMessageAmf3)]
    public class RtmpDataMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;

        public RtmpDataMessageHandler(IRtmpMediaMessageManagerService mediaMessageManager)
        {
            _mediaMessageManager = mediaMessageManager;
        }

        public async Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
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
                RtmpDataMessageConstants.SetDataFrame => await HandleSetDataFrameAsync(peerContext, chunkStreamContext, amfData),
                _ => true
            };
        }

        private async Task<bool> HandleSetDataFrameAsync(
            IRtmpClientPeerContext peerContext,
            IRtmpChunkStreamContext chunkStreamContext,
            object[] amfData)
        {
            var eventName = amfData[1] as string;
            switch (eventName)
            {
                case RtmpDataMessageConstants.OnMetaData:
                    var metaData = amfData[2] as IDictionary<string, object>;
                    return metaData != null ? await HandleOnMetaDataAsync(peerContext, chunkStreamContext, metaData) : true;
                default:
                    return true;
            }
        }

        private Task<bool> HandleOnMetaDataAsync(
            IRtmpClientPeerContext peerContext,
            IRtmpChunkStreamContext chunkStreamContext,
            IDictionary<string, object> metaData)
        {
            var publishStreamContext = peerContext.PublishStreamContext
                ?? throw new InvalidOperationException("Stream is not yet created.");

            CacheStreamMetaData(metaData, publishStreamContext);

            BroadcastMetaDataToSubscribers(peerContext, chunkStreamContext, publishStreamContext);

            return Task.FromResult(true);
        }

        private static void CacheStreamMetaData(IDictionary<string, object> metaData, IRtmpPublishStreamContext publishStreamContext)
        {
            publishStreamContext.StreamMetaData = new PublishStreamMetaData(
                videoFrameRate: (uint)(double)metaData["framerate"],
                videoWidth: (uint)(double)metaData["width"],
                videoHeight: (uint)(double)metaData["height"],
                audioSampleRate: (uint)(double)metaData["audiosamplerate"],
                stereo: (bool)metaData["stereo"]
            );
        }

        private void BroadcastMetaDataToSubscribers(
            IRtmpClientPeerContext peerContext,
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext)
        {
            _mediaMessageManager.SendCachedStreamMetaData(
                peerContext,
                publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId);
        }
    }
}
