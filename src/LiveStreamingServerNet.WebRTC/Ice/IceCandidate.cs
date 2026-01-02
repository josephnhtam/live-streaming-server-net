using LiveStreamingServerNet.WebRTC.Ice.Internal;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice
{
    public record IceCandidate(IPEndPoint EndPoint, IceCandidateType Type, string Foundation, ushort LocalPreference = 65535, int ComponentId = 1)
    {
        public ulong Priority { get; } = IceLogic.CalculateCandidatePriority(Type, LocalPreference, ComponentId);
    }

    internal record LocalIceCandidate(IIceEndPoint IceEndPoint, IPEndPoint BoundEndPoint, IPEndPoint EndPoint, IceCandidateType Type, string Foundation, ushort LocalPreference = 65535, int ComponentId = 1) :
        IceCandidate(EndPoint, Type, Foundation, LocalPreference, ComponentId);

    public record RemoteIceCandidate(IPEndPoint EndPoint, IceCandidateType Type, string Foundation, ushort LocalPreference = 65535, int ComponentId = 1) :
        IceCandidate(EndPoint, Type, Foundation, LocalPreference, ComponentId);
}
