using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    internal interface IRtmpStreamDeletionService
    {
        void DeleteStream(IRtmpClientPeerContext peerContext);
    }
}
