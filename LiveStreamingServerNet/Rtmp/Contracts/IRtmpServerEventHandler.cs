namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpServerEventHandler
    {
        void OnRtmpClientCreated(IRtmpClientPeerContext peerContext);
        void OnRtmpClientDisposed(IRtmpClientPeerContext peerContext);
    }
}
