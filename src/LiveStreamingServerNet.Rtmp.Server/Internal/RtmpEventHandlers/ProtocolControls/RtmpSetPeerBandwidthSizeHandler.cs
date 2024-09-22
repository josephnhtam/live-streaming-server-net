using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.ProtocolControls
{
    [RtmpMessageType(RtmpMessageType.SetPeerBandwidth)]
    internal class RtmpSetPeerBandwidthHandler : IRtmpMessageHandler<IRtmpClientSessionContext>
    {
        public RtmpSetPeerBandwidthHandler() { }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }
    }
}
