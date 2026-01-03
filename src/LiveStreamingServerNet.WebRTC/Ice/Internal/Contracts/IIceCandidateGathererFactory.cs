namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IIceCandidateGathererFactory
    {
        IIceCandidateGatherer Create(string identifier);
    }
}
