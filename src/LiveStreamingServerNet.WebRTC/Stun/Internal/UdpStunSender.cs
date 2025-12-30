using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Udp.Internal.Contracts;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal
{
    public class UdpStunSender : IStunSender
    {
        private readonly IUdpTransport _transport;

        public UdpStunSender(IUdpTransport transport)
        {
            _transport = transport;
        }

        public ValueTask SendAsync(IDataBuffer buffer, IPEndPoint remoteEndpoint, CancellationToken cancellation)
        {
            _transport.SendPacket(buffer.AsReadOnlyMemory(), remoteEndpoint);
            return ValueTask.CompletedTask;
        }
    }
}
