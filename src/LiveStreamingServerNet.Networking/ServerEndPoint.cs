using System.Net;

namespace LiveStreamingServerNet.Networking
{
    /// <summary>
    /// Record representing a server endpoint with security information.
    /// </summary>
    /// <param name="IPEndPoint">IP address and port information.</param>
    /// <param name="IsSecure">Indicates whether connection to endpoint requires secure transport.</param>
    public record ServerEndPoint(IPEndPoint IPEndPoint, bool IsSecure)
    {
        /// <summary>
        /// Implicitly converts IPEndPoint to ServerEndPoint with non-secure transport.
        /// </summary>
        /// <param name="ipEndPoint">IP endpoint to convert.</param>
        /// <returns>New ServerEndPoint instance with IsSecure set to false.</returns>
        public static implicit operator ServerEndPoint(IPEndPoint ipEndPoint) => new(ipEndPoint, false);
    }
}
