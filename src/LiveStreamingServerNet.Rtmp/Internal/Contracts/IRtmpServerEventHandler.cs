using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpServerConnectionEventHandler
    {
        int GetOrder() => 0;

        ValueTask OnRtmpClientCreatedAsync(IEventContext context, IRtmpClientContext clientContext);
        ValueTask OnRtmpClientDisposedAsync(IEventContext context, IRtmpClientContext clientContext);

        ValueTask OnRtmpClientHandshakeCompleteAsync(IEventContext context, IRtmpClientContext clientId);
        ValueTask OnRtmpClientConnectedAsync(IEventContext context, IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    internal interface IRtmpServerStreamEventHandler
    {
        int GetOrder() => 0;

        ValueTask OnRtmpStreamPublishedAsync(IEventContext context, IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, IRtmpClientContext clientContext, string streamPath);
        ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, IRtmpClientContext clientContext, string streamPath);

        ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData);
    }
}
