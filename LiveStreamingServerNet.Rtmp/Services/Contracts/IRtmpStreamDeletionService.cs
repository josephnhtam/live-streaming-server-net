using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    internal interface IRtmpStreamDeletionService
    {
        Task DeleteStream(IRtmpClientPeerContext peerContext);
    }
}
