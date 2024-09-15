using LiveStreamingServerNet.Networking.Internal.Contracts;

namespace LiveStreamingServerNet.Networking.Client.Internal.Contracts
{
    internal interface ITcpClientFactory
    {
        ITcpClientInternal Create();
    }
}
