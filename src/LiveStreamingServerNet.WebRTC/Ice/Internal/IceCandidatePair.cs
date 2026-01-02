using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal class IceCandidatePair
    {
        public LocalIceCandidate LocalCandidate { get; }
        public RemoteIceCandidate RemoteCandidate { get; }
        public string Foundation { get; }
        public ulong Priority { get; }

        public IceCandidatePairState State { get; set; }
        public IceCandidateNominationState NominationState { get; set; }
        public bool IsTriggered { get; set; }

        public IceCandidatePair(LocalIceCandidate localCandidate, RemoteIceCandidate remoteCandidate, bool isLocalControlling)
        {
            LocalCandidate = localCandidate;
            RemoteCandidate = remoteCandidate;

            Foundation = $"{localCandidate.Foundation}:{remoteCandidate.Foundation}";
            Priority = IceLogic.CalculateCandidatePairPriority(localCandidate.Priority, remoteCandidate.Priority, isLocalControlling);

            State = IceCandidatePairState.Frozen;
            NominationState = IceCandidateNominationState.None;
        }

        public Task<(StunMessage, UnknownAttributes?)> SendStunRequestAsync(StunMessage request, CancellationToken cancellation = default)
        {
            return LocalCandidate.IceEndPoint.SendStunRequestAsync(request, RemoteCandidate.EndPoint, cancellation);
        }

        public ValueTask SendStunIndicationAsync(StunMessage indication, CancellationToken cancellation = default)
        {
            return LocalCandidate.IceEndPoint.SendStunIndicationAsync(indication, RemoteCandidate.EndPoint, cancellation);
        }
    }

    internal enum IceCandidatePairState
    {
        Frozen,
        Waiting,
        InProgress,
        Succeeded,
        Failed
    }

    internal enum IceCandidateNominationState
    {
        None,
        Nominating,
        Nominated
    }
}
