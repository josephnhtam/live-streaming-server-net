namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface ISessionFactory
    {
        ISession Create(uint id, ITcpClientInternal tcpClient, ServerEndPoint serverEndPoint);
    }
}
