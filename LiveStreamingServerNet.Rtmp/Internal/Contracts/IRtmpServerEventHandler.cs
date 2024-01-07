namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpServerEventHandler
    {
        Task OnRtmpClientCreatedAsync(IRtmpClientPeerContext peerContext);
        Task OnRtmpClientDisposedAsync(IRtmpClientPeerContext peerContext);
    }
}
