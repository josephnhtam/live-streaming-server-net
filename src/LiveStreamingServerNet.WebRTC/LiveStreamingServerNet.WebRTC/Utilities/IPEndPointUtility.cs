using System.Net;

namespace LiveStreamingServerNet.WebRTC.Utilities
{
    public static class IPEndPointUtility
    {
        public static bool IsEquivalent(this IPEndPoint endPoint1, IPEndPoint endPoint2)
        {
            if (endPoint1.Port != endPoint2.Port)
            {
                return false;
            }

            var a = endPoint1.Address;
            var b = endPoint2.Address;

            if (a.AddressFamily == b.AddressFamily)
            {
                return a.Equals(b);
            }

            if (a.IsIPv4MappedToIPv6)
            {
                a = a.MapToIPv4();
            }

            if (b.IsIPv4MappedToIPv6)
            {
                b = b.MapToIPv4();
            }

            return a.Equals(b);
        }
    }
}
