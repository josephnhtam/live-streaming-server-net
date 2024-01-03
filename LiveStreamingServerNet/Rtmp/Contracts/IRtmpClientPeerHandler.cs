using LiveStreamingServerNet.Newtorking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpClientPeerHandler : IClientPeerHandler
    {
        Task InitializeAsync(IRtmpClientPeerContext peerContext);
    }
}
