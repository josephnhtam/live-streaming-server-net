using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal static class IceUtility
    {
        public static ulong CalculateCandidatePriority(IceCandidateType type, ushort localPreference = 65535, int componentId = 1)
        {
            var typePreference = type switch
            {
                IceCandidateType.Host => 126UL,
                IceCandidateType.PeerReflexive => 110UL,
                IceCandidateType.ServerReflexive => 100UL,
                _ => 0UL
            };

            return ((1UL << 24) * typePreference) + ((256UL << 8) * localPreference) + ((256UL << 8) - (ulong)componentId);
        }

        public static ulong CalculateCandidatePairPriority(ulong localPriority, ulong remotePriority, bool isControlling)
        {
            var g = isControlling ? localPriority : remotePriority;
            var d = isControlling ? remotePriority : localPriority;

            return (1UL << 32) * Math.Min(g, d) + 2 * Math.Max(g, d) + (g > d ? 1UL : 0UL);
        }

        public static bool CanCandidatesPair(IceCandidate local, IceCandidate remote)
        {
            if (local.EndPoint.AddressFamily != remote.EndPoint.AddressFamily)
            {
                return false;
            }

            if (local.EndPoint is { AddressFamily: AddressFamily.InterNetworkV6, Address.IsIPv6LinkLocal: true } &&
                !remote.EndPoint.Address.IsIPv6LinkLocal)
            {
                return false;
            }

            return true;
        }
    }
}
