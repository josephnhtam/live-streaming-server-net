using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IIceAgent
    {
        void OnLocalCandidateGathered(IceCandidate candidate, IStunPeer stunPeer);
        void OnRemoteCandidateReceived(IceCandidate candidate);

        void OnLocalGatheringComplete();
        void OnRemoteEndOfCandidates();
    }
}
