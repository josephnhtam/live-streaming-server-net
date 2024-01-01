using LiveStreamingServerNet.Newtorking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpClientPeerHandler : IClientPeerHandler
    {
        void Initialize(IRtmpClientPeerContext peerContext);
    }
}
