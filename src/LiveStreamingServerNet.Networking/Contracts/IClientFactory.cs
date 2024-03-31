using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Contracts
{
    internal interface IClientFactory
    {
        IClient Create(uint clientId, TcpClient tcpClient);
    }
}
