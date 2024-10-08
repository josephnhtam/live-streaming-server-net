﻿using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.SetPeerBandwidth)]
    internal class RtmpSetPeerBandwidthHandler : IRtmpMessageHandler<IRtmpSessionContext>
    {
        public RtmpSetPeerBandwidthHandler() { }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var bandwidth = payloadBuffer.ReadUInt32BigEndian();
            var limitType = (RtmpBandwidthLimitType)payloadBuffer.ReadByte();
            context.BandwidthLimit = new RtmpBandwidthLimit(bandwidth, limitType);

            return ValueTask.FromResult(true);
        }
    }
}
