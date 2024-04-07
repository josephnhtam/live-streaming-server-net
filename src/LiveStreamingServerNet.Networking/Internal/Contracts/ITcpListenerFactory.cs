using System.Net;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface ITcpListenerFactory
    {
        ITcpListenerInternal Create(IPEndPoint endpoint);
    }
}
