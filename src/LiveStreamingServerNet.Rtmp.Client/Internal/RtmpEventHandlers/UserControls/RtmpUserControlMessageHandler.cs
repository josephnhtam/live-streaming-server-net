using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.UserControls
{
    [RtmpMessageType(RtmpMessageType.UserControlMessage)]
    internal class RtmpUserControlMessageHandler : IRtmpMessageHandler<IRtmpSessionContext>
    {
        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }
    }
}
