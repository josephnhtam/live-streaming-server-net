using System.Net;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets
{
    internal readonly record struct StunResponse(StunMessage Message, UnknownAttributes? Attributes, IPEndPoint RemoteEndPoint, object? State) : IDisposable
    {
        public void Dispose()
        {
            Message.Dispose();
        }
    }
}
