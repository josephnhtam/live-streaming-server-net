using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Utilities
{
    public static class NetworkUtility
    {
        public static List<IPAddress> GetLocalIPAddresses(bool includeLoopback = true)
        {
            var result = new List<IPAddress>();

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.OperationalStatus != OperationalStatus.Up &&
                    adapter.OperationalStatus != OperationalStatus.Unknown)
                {
                    continue;
                }

                if (!includeLoopback &&
                    adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                var properties = adapter.GetIPProperties();
                result.AddRange(properties.UnicastAddresses.Select(info => info.Address));
            }

            return result;
        }

        public static Socket CreateBoundUdpSocket(IPAddress ipAddress, int port = 0)
        {
            var socket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                socket.DontFragment = true;
            }
            catch { }

            socket.Bind(new IPEndPoint(ipAddress, port));

            return socket;
        }

        public static bool IsPrivateIPv4Address(IPAddress address)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
                return false;

            Span<byte> bytes = stackalloc byte[4];
            address.TryWriteBytes(bytes, out _);

            return bytes[0] == 10 ||
                (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                (bytes[0] == 192 && bytes[1] == 168);
        }
    }
}
