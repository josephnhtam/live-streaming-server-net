using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
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
