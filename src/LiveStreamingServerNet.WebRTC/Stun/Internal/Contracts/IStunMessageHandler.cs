using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts
{
    internal interface IStunMessageHandler
    {
        ValueTask<StunMessage?> HandleRequestAsync(StunMessage message, UnknownAttributes? unknownAttributes, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        ValueTask HandleIndicationAsync(StunMessage message, UnknownAttributes? unknownAttributes, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
    }
}
