namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface ISessionHandlerFactory
    {
        ISessionHandler Create(ISessionHandle client);
    }
}
