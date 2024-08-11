using System.Net;

namespace LiveStreamingServerNet.Networking
{
    public record ServerEndPoint(IPEndPoint IPEndPoint, bool IsSecure)
    {
        public static implicit operator ServerEndPoint(IPEndPoint ipEndPoint) => new(ipEndPoint, false);
    }
}
