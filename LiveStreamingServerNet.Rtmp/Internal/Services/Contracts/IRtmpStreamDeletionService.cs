using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpStreamDeletionService
    {
        void DeleteStream(IRtmpClientPeerContext peerContext);
    }
}
