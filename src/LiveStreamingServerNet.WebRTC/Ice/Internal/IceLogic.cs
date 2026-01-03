using LiveStreamingServerNet.WebRTC.Utilities;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal static class IceLogic
    {
        public static uint CalculateCandidatePriority(IceCandidateType type, ushort localPreference = 65535, int componentId = 1)
        {
            var typePreference = type switch
            {
                IceCandidateType.Host => 126,
                IceCandidateType.PeerReflexive => 110,
                IceCandidateType.ServerReflexive => 100,
                _ => 0
            };

            return (uint)(((1 << 24) * typePreference) + ((1 << 8) * localPreference) + (256 - componentId));
        }

        public static ulong CalculateCandidatePairPriority(uint localPriority, uint remotePriority, bool isControlling)
        {
            var g = isControlling ? localPriority : remotePriority;
            var d = isControlling ? remotePriority : localPriority;

            return (1UL << 32) * Math.Min(g, d) + 2 * Math.Max(g, d) + (g > d ? 1UL : 0UL);
        }

        public static int GetLocalPreferenceScore(IPAddress address)
        {
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

        public static bool CanPairCandidates(LocalIceCandidate local, RemoteIceCandidate remote)
        {
            if (local.BoundEndPoint.AddressFamily != remote.EndPoint.AddressFamily)
            {
                return false;
            }

            if (local.BoundEndPoint.AddressFamily == AddressFamily.InterNetworkV6 &&
                local.BoundEndPoint.Address.IsIPv6LinkLocal != remote.EndPoint.Address.IsIPv6LinkLocal)
            {
                return false;
            }

            return true;
        }
    }
}
