using System.Net;

namespace LiveStreamingServerNet.Networking.Server.Contracts
{
    /// <summary>
    /// Represents a TCP listener bound to a local endpoint.
    /// </summary>
    public interface ITcpListener
    {
        /// <summary>
        /// Gets the local endpoint address and port that listener is bound to.
        /// </summary>
        EndPoint LocalEndpoint { get; }

        /// <summary>
        /// Determines if there are pending connection requests.
        /// </summary>
        /// <returns>True if connections are pending, false otherwise.</returns>
        bool Pending();
    }
}
