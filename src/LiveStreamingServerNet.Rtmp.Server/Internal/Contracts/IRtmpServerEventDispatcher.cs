namespace LiveStreamingServerNet.Rtmp.Server.Internal.Contracts
{
    internal interface IRtmpServerConnectionEventDispatcher
    {
        ValueTask RtmpClientCreatedAsync(IRtmpClientSessionContext clientContext);
        ValueTask RtmpClientDisposingAsync(IRtmpClientSessionContext clientContext);
        ValueTask RtmpClientDisposedAsync(IRtmpClientSessionContext clientContext);

        ValueTask RtmpClientHandshakeCompleteAsync(IRtmpClientSessionContext clientContext);
        ValueTask RtmpClientConnectedAsync(IRtmpClientSessionContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    internal interface IRtmpServerStreamEventDispatcher
    {
        ValueTask RtmpStreamPublishedAsync(IRtmpPublishStreamContext publishStreamContext);
        ValueTask RtmpStreamUnpublishedAsync(IRtmpPublishStreamContext publishStreamContext, bool allowContinuation);
        ValueTask RtmpStreamMetaDataReceivedAsync(IRtmpPublishStreamContext publishStreamContext);

        ValueTask RtmpStreamSubscribedAsync(IRtmpSubscribeStreamContext subscribeStreamContext);
        ValueTask RtmpStreamUnsubscribedAsync(IRtmpSubscribeStreamContext subscribeStreamContext);
    }
}
