using LiveStreamingServerNet.WebRTC.Ice.Contracts;
using LiveStreamingServerNet.WebRTC.Utilities;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal class DefaultLocalPreferenceScoreProvider : ILocalPreferenceScoreProvider
    {
        public static DefaultLocalPreferenceScoreProvider Instance { get; } = new DefaultLocalPreferenceScoreProvider();

        public ushort ProvideLocalPreferenceScore(LocalIPAddressInfo localAddress)
        {
            var address = localAddress.Address;

            if (IPAddress.IsLoopback(address))
                return 1000;

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                if (NetworkUtility.IsPrivateIPv4Address(address))
                    return 500;

                return 400;
            }

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (address.IsIPv6LinkLocal)
                    return 100;

                return 200;
            }

            return 0;
        }
    }
}
