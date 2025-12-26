using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Ice.Configurations;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal class IceAgent : IIceAgent
    {
        private readonly IceCredentials _credentials;
        private readonly IceAgentConfiguration _config;
        private readonly ulong _tieBreaker;

        public IceAgent(IceCredentials credentials, IceAgentConfiguration config)
        {
            _credentials = credentials;
            _config = config;
            _tieBreaker = RandomNumberUtility.GetRandomUInt64();
        }

        public void OnLocalCandidateGathered(IceCandidate candidate, IStunPeer stunPeer)
        {

        }

        public void OnRemoteCandidateReceived(IceCandidate candidate)
        {

        }

        public void OnLocalGatheringComplete()
        {

        }

        public void OnRemoteEndOfCandidates()
        {

        }
    }
}
