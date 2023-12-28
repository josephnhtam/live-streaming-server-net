using LiveStreamingServer.Newtorking.Contracts;

namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IRtmpClientPeerHandler : IClientPeerHandler
    {
        void Initialize(IRtmpClientPeerContext peerContext);
    }
}
