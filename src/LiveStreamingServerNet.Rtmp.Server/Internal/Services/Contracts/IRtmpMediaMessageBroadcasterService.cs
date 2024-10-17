using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageBroadcasterService
    {
        ValueTask BroadcastMediaMessageAsync(
            IRtmpPublishStreamContext publishStreamContext,
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts,
            MediaType mediaType,
            uint timestamp,
            bool isSkippable,
            IDataBuffer payloadBuffer);

        void RegisterClient(IRtmpClientSessionContext clientContext);
        ValueTask UnregisterClientAsync(IRtmpClientSessionContext clientContext);
    }
}
