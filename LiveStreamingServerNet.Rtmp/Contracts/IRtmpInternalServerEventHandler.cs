namespace LiveStreamingServerNet.Rtmp.Contracts
{
    internal interface IRtmpInternalServerEventHandler
    {
        Task OnRtmpClientCreatedAsync(IRtmpClientPeerContext peerContext);
        Task OnRtmpClientDisposedAsync(IRtmpClientPeerContext peerContext);
    }
}
