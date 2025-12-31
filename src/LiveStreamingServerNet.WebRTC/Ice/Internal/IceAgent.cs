using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Ice.Configurations;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal partial class IceAgent : IIceAgent
    {
        private readonly IceCredentials _credentials;
        private readonly IceAgentConfiguration _config;
        private readonly ulong _tieBreaker;
        private readonly object _syncLock = new object();

        public IceRole Role { get; private set; }
        public IceConnectionState ConnectionState { get; private set; }
        public event Action<IceConnectionState>? OnStateChanged;

        public IceAgent(
            IceRole role,
            IceCredentials credentials,
            IceAgentConfiguration config,
            ulong? tieBreaker = null)
        {
            Role = role;
            _credentials = credentials;
            _config = config;

            _tieBreaker = tieBreaker ?? RandomNumberUtility.GetRandomUInt64();
            ConnectionState = IceConnectionState.New;
        }

        private void SetState(IceConnectionState newState)
        {
            lock (_syncLock)
            {
                if (ConnectionState == newState)
                    return;

                ConnectionState = newState;
                OnStateChanged?.Invoke(newState);
            }
        }

        public void OnLocalCandidateGathered(RemoteIceCandidate candidate, Socket socket)
        {

        }

        public void OnRemoteCandidateReceived(RemoteIceCandidate candidate)
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
