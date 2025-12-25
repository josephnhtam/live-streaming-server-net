using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Contracts
{
    internal interface IStunPeer
    {
        Task<(StunMessage, UnknownAttributes?)> SendRequestAsync(ushort method, IList<IStunAttribute> attributes, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        Task SendIndicationAsync(ushort method, IList<IStunAttribute> attributes, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
    }
}
