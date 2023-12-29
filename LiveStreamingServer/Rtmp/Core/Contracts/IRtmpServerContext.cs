namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IRtmpServerContext
    {
        string? GetPublishStreamPath(IRtmpClientPeerContext peerContext);
        IRtmpClientPeerContext? GetPublishingClientPeerContext(string publishStreamPath);
        StartPublishingStreamResult StartPublishingStream(string publishStreamPath, IRtmpClientPeerContext peerContext);
        void StopPublishingStream(string publishStreamPath);
        void RemoveClientPeerContext(IRtmpClientPeerContext peerContext);
    }
}
