using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.Data
{
    [RtmpMessageType(RtmpMessageType.DataMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.DataMessageAmf3)]
    internal class RtmpDataMessageHandler : IRtmpMessageHandler<IRtmpSessionContext>
    {
        private readonly ILogger<RtmpDataMessageHandler> _logger;

        public RtmpDataMessageHandler(ILogger<RtmpDataMessageHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
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
                RtmpDataMessageConstants.OnMetaData => await HandleOnMetaDataAsync(context, chunkStreamContext, amfData),
                _ => true
            };
        }

        private ValueTask<bool> HandleOnMetaDataAsync(
           IRtmpSessionContext context,
           IRtmpChunkStreamContext chunkStreamContext,
           object[] amfData)
        {
            var metaData = amfData[1] as IDictionary<string, object>;

            if (metaData == null)
                return ValueTask.FromResult(true);

            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var subscribeStreamContext = context.GetStreamContext(streamId)?.SubscribeContext;

            if (subscribeStreamContext != null)
                subscribeStreamContext.StreamMetaData = new Dictionary<string, object>(metaData);

            return ValueTask.FromResult(true);
        }
    }
}
