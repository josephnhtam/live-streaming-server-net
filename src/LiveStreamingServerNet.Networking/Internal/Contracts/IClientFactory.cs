namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface IClientFactory
    {
        IClient Create(uint clientId, ITcpClientInternal tcpClient);
    }
}
