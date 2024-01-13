namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpServerConnectionEventHandler
    {
        Task OnRtmpClientCreatedAsync(IRtmpClientContext clientContext);
        Task OnRtmpClientDisposedAsync(IRtmpClientContext clientContext);

        Task OnRtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId);
        Task OnRtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    internal interface IRtmpServerStreamEventHandler
    {
        Task OnRtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task OnRtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath);
        Task OnRtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task OnRtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath);

        Task OnRtmpStreamMetaDataReceived(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData);
    }
}
