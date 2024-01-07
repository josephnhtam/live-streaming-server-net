using LiveStreamingServerNet.Newtorking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpClientPeerHandler : IClientPeerHandler
    {
        Task InitializeAsync(IRtmpClientPeerContext peerContext);
    }
}
