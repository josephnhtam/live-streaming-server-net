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

        private readonly IIceCandidateGatherer _candidateGatherer;
        private readonly CheckList _checkList;

        public IceRole Role { get; private set; }
        public IceConnectionState ConnectionState { get; private set; }
        public event Action<IceConnectionState>? OnStateChanged;

        public IceAgent(
            IceRole role,
            IceCredentials credentials,
            IceAgentConfiguration config,
            IceCandidateGathererFactory candidateGathererFactory,
            ulong? tieBreaker = null)
        {
            Role = role;
            _credentials = credentials;
            _config = config;

            _tieBreaker = tieBreaker ?? RandomNumberUtility.GetRandomUInt64();
            ConnectionState = IceConnectionState.New;

            _candidateGatherer = candidateGathererFactory.Create();
            _candidateGatherer.OnGathered += OnLocalCandidateGathered;

            _checkList = new CheckList(this);
        }

        public bool Start()
        {
            if (!TryTransitionTo(IceConnectionState.Checking, IceConnectionState.New))
                return false;

            _candidateGatherer.StartGathering();
            return true;
        }

        public async ValueTask<bool> StopAsync()
        {
            if (!TryTransitionTo(IceConnectionState.Closed,
                IceConnectionState.Checking,
                IceConnectionState.Connected,
                IceConnectionState.Completed,
                IceConnectionState.Failed))
            {
                return false;
            }

            if (_candidateGatherer != null)
            {
                _candidateGatherer.OnGathered -= OnLocalCandidateGathered;

                await ErrorBoundary.ExecuteAsync(async () =>
                    await _candidateGatherer.StopGatheringAsync().ConfigureAwait(false)
                ).ConfigureAwait(false);
            }

            return true;
        }

        private void OnLocalCandidateGathered(object gatherer, LocalIceCandidate? candidate)
        {
            if (_checkList.AddLocalCandidate(candidate) && candidate != null)
            {
                candidate.IceEndPoint.SetStunMessageHandler(new StunMessageHandler(this));
            }
        }

        public void AddRemoteCandidate(RemoteIceCandidate? candidate)
        {
            _checkList.AddRemoteCandidate(candidate);
        }

        private bool TryTransitionTo(IceConnectionState newState, params IceConnectionState[] expected)
        {
            lock (_syncLock)
            {
                if (expected.Length > 0 && !expected.Contains(ConnectionState))
                    return false;

                if (ConnectionState == newState)
                    return false;

                ConnectionState = newState;
                OnStateChanged?.Invoke(newState);
                return true;
            }
        }
    }
}
