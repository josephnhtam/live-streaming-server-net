namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    [Flags]
    public enum IceCandidateTypeFlag
    {
        All = -1,
        Host = 1 << 0,
        Relayed = 1 << 1,
        ServerReflexive = 1 << 2,
    }
}
