namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpServerEventHandler
    {
        Task OnRtmpClientCreatedAsync(IRtmpClientPeerContext peerContext);
        Task OnRtmpClientDisposedAsync(IRtmpClientPeerContext peerContext);
    }
}
