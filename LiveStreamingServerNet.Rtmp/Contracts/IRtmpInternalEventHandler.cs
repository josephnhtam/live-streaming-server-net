namespace LiveStreamingServerNet.Rtmp.Contracts
{
    internal interface IRtmpInternalEventHandler
    {
        Task OnRtmpClientCreatedAsync(IRtmpClientPeerContext peerContext);
        Task OnRtmpClientDisposedAsync(IRtmpClientPeerContext peerContext);
    }
}
