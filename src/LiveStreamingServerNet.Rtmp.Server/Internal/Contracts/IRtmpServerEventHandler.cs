using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Contracts
{
    internal interface IRtmpServerConnectionEventHandler
    {
        int GetOrder() => 0;

        ValueTask OnRtmpClientCreatedAsync(IEventContext context, IRtmpClientSessionContext clientContext);
        ValueTask OnRtmpClientDisposingAsync(IEventContext context, IRtmpClientSessionContext clientContext);
        ValueTask OnRtmpClientDisposedAsync(IEventContext context, IRtmpClientSessionContext clientContext);

        ValueTask OnRtmpClientHandshakeCompleteAsync(IEventContext context, IRtmpClientSessionContext clientContext);
        ValueTask OnRtmpClientConnectedAsync(IEventContext context, IRtmpClientSessionContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    internal interface IRtmpServerStreamEventHandler
    {
        int GetOrder() => 0;

        ValueTask OnRtmpStreamPublishedAsync(IEventContext context, IRtmpPublishStreamContext publishStreamContext);
        ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, IRtmpPublishStreamContext publishStreamContext);
        ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, IRtmpPublishStreamContext publishStreamContext);

        ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, IRtmpSubscribeStreamContext subscribeStreamContext);
        ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, IRtmpSubscribeStreamContext subscribeStreamContext);
    }
}
