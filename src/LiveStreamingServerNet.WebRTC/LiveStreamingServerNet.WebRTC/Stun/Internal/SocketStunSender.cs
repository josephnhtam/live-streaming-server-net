using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal
{
    internal class SocketStunSender : IStunSender
    {
        private readonly Socket _socket;

        public SocketStunSender(Socket socket)
        {
            _socket = socket;
        }

        public async ValueTask SendAsync(IDataBuffer buffer, IPEndPoint remoteEndpoint, CancellationToken cancellation)
        {
            await _socket.SendToAsync(buffer.AsMemory(), SocketFlags.None, remoteEndpoint, cancellation);
        }
    }
}
