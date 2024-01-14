using System.Net;

namespace LiveStreamingServerNet.Networking
{
    public record ServerEndPoint(IPEndPoint LocalEndPoint, bool IsSecure)
    {
        public static implicit operator ServerEndPoint(IPEndPoint localEndPoint) => new(localEndPoint, false);
    }
}
