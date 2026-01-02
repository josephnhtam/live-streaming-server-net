namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IceCandidateGathererFactory
    {
        IIceCandidateGatherer Create();
    }
}
