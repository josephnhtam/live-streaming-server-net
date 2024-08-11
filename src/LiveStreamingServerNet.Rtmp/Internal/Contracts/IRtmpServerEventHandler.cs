using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpServerConnectionEventHandler
    {
        int GetOrder() => 0;

        ValueTask OnRtmpClientCreatedAsync(IEventContext context, IRtmpClientSessionContext clientContext);
        ValueTask OnRtmpClientDisposedAsync(IEventContext context, IRtmpClientSessionContext clientContext);

        ValueTask OnRtmpClientHandshakeCompleteAsync(IEventContext context, IRtmpClientSessionContext clientContext);
        ValueTask OnRtmpClientConnectedAsync(IEventContext context, IRtmpClientSessionContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    internal interface IRtmpServerStreamEventHandler
    {
        int GetOrder() => 0;

        ValueTask OnRtmpStreamPublishedAsync(IEventContext context, IRtmpClientSessionContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, IRtmpClientSessionContext clientContext, string streamPath);
        ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, IRtmpClientSessionContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, IRtmpClientSessionContext clientContext, string streamPath);

        ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, IRtmpClientSessionContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData);
    }
}
