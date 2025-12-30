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

                if (includeLoopback &&
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
    }
}
