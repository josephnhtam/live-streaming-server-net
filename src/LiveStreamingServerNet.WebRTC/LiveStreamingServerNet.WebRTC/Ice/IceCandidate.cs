using LiveStreamingServerNet.WebRTC.Ice.Internal;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice
{
    public record IceCandidate(IPEndPoint EndPoint, IceCandidateType Type, string Foundation, ushort LocalPreference = 65535, int ComponentId = 1)
    {
        public ulong Priority { get; } = IceUtility.CalculateCandidatePriority(Type, LocalPreference, ComponentId);
    }
}
