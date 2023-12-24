namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IRtmpClientPeerContext
    {
        RtmpClientPeerState State { get; set; }
    }
}
