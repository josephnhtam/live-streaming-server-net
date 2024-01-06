using LiveStreamingServerNet.Newtorking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    internal interface IRtmpClientPeerHandler : IClientPeerHandler
    {
        Task InitializeAsync(IRtmpClientPeerContext peerContext);
    }
}
