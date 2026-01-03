using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Udp.Internal;
using LiveStreamingServerNet.WebRTC.Udp.Internal.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    public class UdpTransportFactory : IUdpTransportFactory
    {
        private readonly IDataBufferPool _bufferPool;

        public UdpTransportFactory(IDataBufferPool bufferPool)
        {
            _bufferPool = bufferPool;
        }

        public IUdpTransport Create(Socket socket)
        {
            return new UdpTransport(socket, _bufferPool);
        }
    }
}
