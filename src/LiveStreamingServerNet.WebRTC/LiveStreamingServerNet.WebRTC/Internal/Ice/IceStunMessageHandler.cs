using LiveStreamingServerNet.WebRTC.Internal.Stun.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Internal.Ice
{
    internal class IceStunMessageHandler : IStunMessageHandler
    {
        public ValueTask<StunMessage> HandleRequestAsync(StunMessage message, UnknownAttributes? unknownAttributes, IPEndPoint remoteEndPoint, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask HandleIndicationAsync(StunMessage message, UnknownAttributes? unknownAttributes, IPEndPoint remoteEndPoint, CancellationToken cancellation = default)
        {
            return ValueTask.CompletedTask;
        }
    }
}
