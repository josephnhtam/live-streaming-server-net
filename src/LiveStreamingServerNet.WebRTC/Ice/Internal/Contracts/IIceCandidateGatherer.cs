namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IIceCandidateGatherer
    {
        event EventHandler<LocalIceCandidate?> OnGathered;

        bool StartGathering();
        ValueTask<bool> StopGatheringAsync();
    }
}
