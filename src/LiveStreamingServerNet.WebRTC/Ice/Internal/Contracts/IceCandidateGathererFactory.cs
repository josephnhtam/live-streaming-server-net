namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IceCandidateGatherFactory
    {
        IIceCandidateGatherer Create();
    }
}
