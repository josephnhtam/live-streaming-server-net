using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Udp.Internal.Contracts;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IStunAgentFactory
    {
        IStunAgent Create(IUdpTransport transport);
    }
}
