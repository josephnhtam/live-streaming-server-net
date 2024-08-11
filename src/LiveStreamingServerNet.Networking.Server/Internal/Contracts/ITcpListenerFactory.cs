using System.Net;

namespace LiveStreamingServerNet.Networking.Server.Internal.Contracts
{
    internal interface ITcpListenerFactory
    {
        ITcpListenerInternal Create(IPEndPoint endpoint);
    }
}
