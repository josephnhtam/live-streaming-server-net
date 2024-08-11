using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageBroadcasterService
    {
        ValueTask BroadcastMediaMessageAsync(
            IRtmpPublishStreamContext publishStreamContext,
            IReadOnlyList<IRtmpClientSessionContext> subscribers,
            MediaType mediaType,
            uint timestamp,
            bool isSkippable,
            IDataBuffer payloadBuffer);

        void RegisterClient(IRtmpClientSessionContext clientContext);
        void UnregisterClient(IRtmpClientSessionContext clientContext);
    }
}
