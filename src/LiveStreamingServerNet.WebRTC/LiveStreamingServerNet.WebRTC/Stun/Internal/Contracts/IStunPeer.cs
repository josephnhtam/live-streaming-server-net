using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts
{
    internal interface IStunPeer
    {
        void SetMessageHandler(IStunMessageHandler? handler);
        Task<(StunMessage, UnknownAttributes?)> SendRequestAsync(StunMessage request, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        Task SendIndicationAsync(StunMessage indication, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
    }
}
