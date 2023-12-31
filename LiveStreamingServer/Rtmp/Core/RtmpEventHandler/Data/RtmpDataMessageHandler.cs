using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Extensions;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Data
{
    [RtmpMessageType(RtmpMessageType.DataMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.DataMessageAmf3)]
    public class RtmpDataMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpServerContext _serverContext;
        private readonly ILogger _logger;

        public RtmpDataMessageHandler(IRtmpServerContext serverContext, ILogger<RtmpDataMessageHandler> logger)
        {
            _serverContext = serverContext;
            _logger = logger;
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
                RtmpDataMessageConstants.SetDataFrame => await HandleSetDataFrameAsync(peerContext, amfData),
                _ => true
            };
        }

        private async Task<bool> HandleSetDataFrameAsync(IRtmpClientPeerContext peerContext, object[] amfData)
        {
            var eventName = amfData[1] as string;
            switch (eventName)
            {
                case RtmpDataMessageConstants.OnMetaData:
                    var metaData = amfData[2] as IDictionary<string, object>;
                    return metaData != null ? await HandleOnMetaDataAsync(peerContext, metaData) : true;
                default:
                    return true;
            }
        }

        private Task<bool> HandleOnMetaDataAsync(IRtmpClientPeerContext peerContext, IDictionary<string, object> metaData)
        {
            var publishStreamContext = peerContext.PublishStreamContext
                ?? throw new InvalidOperationException("Stream is not yet created.");

            publishStreamContext.StreamMetaData = new PublishStreamMetaData(
                videoFrameRate: (uint)(double)metaData["framerate"],
                videoWidth: (uint)(double)metaData["width"],
                videoHeight: (uint)(double)metaData["height"],
                audioSampleRate: (uint)(double)metaData["audiosamplerate"],
                stereo: (bool)metaData["stereo"]
            );

            BroadcastMetaDataToSubscribers(peerContext, metaData);

            return Task.FromResult(true);
        }

        private void BroadcastMetaDataToSubscribers(IRtmpClientPeerContext peerContext, IDictionary<string, object> metaData)
        {

        }
    }
}
