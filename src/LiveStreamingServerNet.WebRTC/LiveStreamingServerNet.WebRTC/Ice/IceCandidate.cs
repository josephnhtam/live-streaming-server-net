using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice
{
    public record IceCandidate(IPEndPoint EndPoint, IceCandidateType Type, string Foundation, int ComponentId = 1, ushort LocalPreference = 65535)
    {
        public ulong Priority { get; } = IceUtility.CalculateCandidatePriority(Type, ComponentId, LocalPreference);
    }
}
