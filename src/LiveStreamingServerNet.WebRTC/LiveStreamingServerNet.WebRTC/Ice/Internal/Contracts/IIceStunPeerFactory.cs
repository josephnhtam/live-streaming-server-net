using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IIceStunPeerFactory
    {
        IStunPeer Create(Socket socket);
    }
}
