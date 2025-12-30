using LiveStreamingServerNet.WebRTC.Udp.Internal.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    public interface IUdpTransportFactory
    {
        IUdpTransport Create(Socket socket);
    }
}
