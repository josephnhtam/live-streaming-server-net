using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.UserControls
{
    [RtmpMessageType(RtmpMessageType.UserControlMessage)]
    internal class RtmpUserControlMessageHandler : IRtmpMessageHandler
    {
        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }
    }
}
