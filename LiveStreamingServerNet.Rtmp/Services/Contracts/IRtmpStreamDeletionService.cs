using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    public interface IRtmpStreamDeletionService
    {
        Task DeleteStream(IRtmpClientPeerContext peerContext);
    }
}
