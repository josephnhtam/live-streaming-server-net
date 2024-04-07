using System.Net;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface ITcpListener
    {
        EndPoint LocalEndpoint { get; }
        bool Pending();
    }
}
