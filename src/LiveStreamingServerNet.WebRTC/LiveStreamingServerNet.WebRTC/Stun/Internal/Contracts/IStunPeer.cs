using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts
{
    internal interface IStunPeer
    {
        Task<(StunMessage, UnknownAttributes?)> SendRequestAsync(ushort method, IList<IStunAttribute> attributes, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        Task SendIndicationAsync(ushort method, IList<IStunAttribute> attributes, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
    }
}
