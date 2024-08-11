using System.Net;

namespace LiveStreamingServerNet.Networking.Server.Contracts
{
    public interface ITcpListener
    {
        EndPoint LocalEndpoint { get; }
        bool Pending();
    }
}
