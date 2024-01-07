using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface IClientHandlerFactory
    {
        IClientHandler CreateClientHandler(IClientHandle client);
    }
}
