namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpServerConnectionEventDispatcher
    {
        Task RtmpClientCreatedAsync(IRtmpClientContext clientContext);
        Task RtmpClientDisposedAsync(IRtmpClientContext clientContext);

        Task RtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId);
        Task RtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    internal interface IRtmpServerStreamEventDispatcher
    {
        Task RtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task RtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath);
        Task RtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task RtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath);

        Task RtmpStreamMetaDataReceived(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData);
    }
}
