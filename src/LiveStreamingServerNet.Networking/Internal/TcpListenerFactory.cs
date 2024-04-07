using LiveStreamingServerNet.Networking.Internal.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class TcpListenerFactory : ITcpListenerFactory
    {
        public ITcpListenerInternal Create(IPEndPoint endpoint)
        {
            return new TcpListenerWrapper(new TcpListener(endpoint));
        }
    }
}
