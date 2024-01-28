namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IClientHandlerFactory
    {
        IClientHandler CreateClientHandler();
    }
}
