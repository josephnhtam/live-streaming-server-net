namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpServerConnectionEventHandler
    {
        ValueTask OnRtmpClientCreatedAsync(IRtmpClientContext clientContext);
        ValueTask OnRtmpClientDisposedAsync(IRtmpClientContext clientContext);

        ValueTask OnRtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId);
        ValueTask OnRtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    internal interface IRtmpServerStreamEventHandler
    {
        ValueTask OnRtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask OnRtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath);
        ValueTask OnRtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask OnRtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath);

        ValueTask OnRtmpStreamMetaDataReceivedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData);
    }
}
