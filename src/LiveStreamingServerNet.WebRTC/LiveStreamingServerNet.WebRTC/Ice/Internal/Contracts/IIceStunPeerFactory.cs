using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IStunPeerFactory
    {
        IStunPeer Create(Socket socket);
    }
}
