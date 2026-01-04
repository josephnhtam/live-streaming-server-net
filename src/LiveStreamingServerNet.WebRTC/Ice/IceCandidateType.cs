namespace LiveStreamingServerNet.WebRTC.Ice
{
    public enum IceCandidateType
    {
        Host = 1 << 0,
        Relayed = 1 << 1,
        ServerReflexive = 1 << 2,
        PeerReflexive = 1 << 3
    }
}
