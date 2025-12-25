using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Contracts
{
    internal interface IStunMessageHandler
    {
        ValueTask<StunMessage> HandleRequestAsync(StunMessage message, UnknownAttributes? unknownAttributes, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
        ValueTask HandleIndicationAsync(StunMessage message, UnknownAttributes? unknownAttributes, IPEndPoint remoteEndPoint, CancellationToken cancellation = default);
    }
}
