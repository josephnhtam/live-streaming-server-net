namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpServerConnectionEventDispatcher
    {
        ValueTask RtmpClientCreatedAsync(IRtmpClientSessionContext clientContext);
        ValueTask RtmpClientDisposedAsync(IRtmpClientSessionContext clientContext);

        ValueTask RtmpClientHandshakeCompleteAsync(IRtmpClientSessionContext clientContext);
        ValueTask RtmpClientConnectedAsync(IRtmpClientSessionContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    internal interface IRtmpServerStreamEventDispatcher
    {
        ValueTask RtmpStreamPublishedAsync(IRtmpClientSessionContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask RtmpStreamUnpublishedAsync(IRtmpClientSessionContext clientContext, string streamPath);
        ValueTask RtmpStreamSubscribedAsync(IRtmpClientSessionContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask RtmpStreamUnsubscribedAsync(IRtmpClientSessionContext clientContext, string streamPath);

        ValueTask RtmpStreamMetaDataReceivedAsync(IRtmpClientSessionContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData);
    }
}
