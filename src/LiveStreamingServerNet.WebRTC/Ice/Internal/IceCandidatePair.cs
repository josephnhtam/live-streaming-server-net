namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal class IceCandidatePair
    {
        public LocalIceCandidate LocalCandidate { get; }
        public RemoteIceCandidate RemoteCandidate { get; }
        public string Foundation { get; }
        public ulong Priority { get; set; }

        public IceCandidatePairState State { get; set; }

        public IceCandidatePair(LocalIceCandidate localCandidate, RemoteIceCandidate remoteCandidate, bool isLocalControlling)
        {
            LocalCandidate = localCandidate;
            RemoteCandidate = remoteCandidate;

            Foundation = $"{localCandidate.Foundation}:{remoteCandidate.Foundation}";
            Priority = IceLogic.CalculateCandidatePairPriority(localCandidate.Priority, remoteCandidate.Priority, isLocalControlling);

            State = IceCandidatePairState.Frozen;
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
}
