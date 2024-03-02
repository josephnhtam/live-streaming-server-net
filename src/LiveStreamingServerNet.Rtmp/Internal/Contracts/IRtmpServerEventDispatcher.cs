namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpServerConnectionEventDispatcher
    {
        ValueTask RtmpClientCreatedAsync(IRtmpClientContext clientContext);
        ValueTask RtmpClientDisposedAsync(IRtmpClientContext clientContext);

        ValueTask RtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId);
        ValueTask RtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    internal interface IRtmpServerStreamEventDispatcher
    {
        ValueTask RtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask RtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath);
        ValueTask RtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask RtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath);

        ValueTask RtmpStreamMetaDataReceivedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData);
    }
}
