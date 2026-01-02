using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Ice.Configurations;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Ice.Internal.StunAttributes;
using LiveStreamingServerNet.WebRTC.Stun.Internal;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using System.Collections.Concurrent;

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
        private readonly CancellationTokenSource _cts;
        private readonly HashSet<IceCandidatePair> _validPairs;
        private readonly ConcurrentDictionary<Task, object?> _connectivityCheckTasks;

        private bool _localGatheringComplete;
        private bool _remoteGatheringComplete;
        private IceCandidatePair? _selectedPair;

        private Task? _checkerTask;
        private Task? _keepaliveTask;
        private Task? _nominatingTask;

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
            _cts = new CancellationTokenSource();
            _validPairs = new HashSet<IceCandidatePair>();
            _connectivityCheckTasks = new ConcurrentDictionary<Task, object?>();
        }

        public bool Start()
        {
            if (!TryTransitionTo(IceConnectionState.Checking, expected: IceConnectionStateFlag.New))
                return false;

            StartGathering();
            _checkerTask = Task.Run(() => CheckerLoopAsync(_cts.Token));
            _keepaliveTask = Task.Run(() => KeepaliveLoopAsync(_cts.Token));
            return true;
        }

        public async ValueTask<bool> StopAsync()
        {
            if (!TryTransitionTo(IceConnectionState.Closed))
            {
                return false;
            }

            _cts.Cancel();
            await StopGatheringAsync().ConfigureAwait(false);
            return true;
        }

        private async ValueTask<bool> FailAsync()
        {
            if (!TryTransitionTo(IceConnectionState.Failed, excluded: IceConnectionStateFlag.Closed))
            {
                return false;
            }

            _cts.Cancel();
            await StopGatheringAsync().ConfigureAwait(false);
            return true;
        }

        private void StartGathering()
        {
            _candidateGatherer.StartGathering();
        }

        private async ValueTask StopGatheringAsync()
        {
            await ErrorBoundary.ExecuteAsync(async () =>
                await _candidateGatherer.StopGatheringAsync().ConfigureAwait(false)
            ).ConfigureAwait(false);
        }

        private async Task CheckerLoopAsync(CancellationToken cancellation)
        {
            var checkInterval = TimeSpanUtility.Max(TimeSpan.FromMilliseconds(50), _config.ConnectivityCheckInterval);
            await RunActiveAgentLoopAsync(checkInterval, CheckNextPair, cancellation).ConfigureAwait(false);

            return;

            void CheckNextPair()
            {
                lock (_syncLock)
                {
                    if (!IsConnectivityCheckMaxConcurrencyReached())
                    {
                        var pairToCheck = _checkList.GetNextPair();

                        if (pairToCheck != null)
                        {
                            ScheduleConnectivityCheck(pairToCheck, ConnectivityCheckReason.Check);
                            return;
                        }
                    }

                    if (CheckCompletion())
                    {
                        return;
                    }

                    TryNominateValidPair();
                }
            }
        }

        private async Task KeepaliveLoopAsync(CancellationToken cancellation)
        {
            var keepAliveInterval = TimeSpanUtility.Max(TimeSpan.FromMilliseconds(1000), _config.KeepAliveInterval);
            await RunActiveAgentLoopAsync(keepAliveInterval, KeepValidPairsAlive, cancellation).ConfigureAwait(false);

            return;

            void KeepValidPairsAlive()
            {
                lock (_syncLock)
                {
                    foreach (var pair in _validPairs)
                    {
                        ScheduleConnectivityCheck(pair, ConnectivityCheckReason.KeepAlive);
                    }
                }
            }
        }

        private async Task RunActiveAgentLoopAsync(TimeSpan interval, Action action, CancellationToken cancellation)
        {
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    await Task.Delay(interval, cancellation).ConfigureAwait(false);

                    lock (_syncLock)
                    {
                        if (ConnectionState is IceConnectionState.Failed or IceConnectionState.Closed)
                            return;

                        action();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            catch (Exception ex)
            {
                await FailAsync().ConfigureAwait(false);
            }
        }

        private bool CheckCompletion()
        {
            lock (_syncLock)
            {
                if (IsFailed())
                {
                    TryTransitionTo(IceConnectionState.Failed, expected: IceConnectionStateFlag.Checking | IceConnectionStateFlag.Connected);
                    return true;
                }

                if (IsCompleted())
                {
                    TryTransitionTo(IceConnectionState.Completed, expected: IceConnectionStateFlag.Checking | IceConnectionStateFlag.Connected);
                    return false;
                }

                return false;
            }

            bool IsCompleted() =>
                _localGatheringComplete && _remoteGatheringComplete &&
                _selectedPair != null && _checkList.AllPairsChecked();

            bool IsFailed() =>
                _localGatheringComplete && _remoteGatheringComplete &&
                _selectedPair == null && _checkList.AllPairsChecked() && !_validPairs.Any();
        }

        private void SelectPair(IceCandidatePair pair)
        {
            lock (_syncLock)
            {
                foreach (var validPair in _validPairs.Where(p =>
                    p != pair && p.NominationState == IceCandidateNominationState.Nominated))
                {
                    validPair.NominationState = IceCandidateNominationState.None;
                }

                pair.NominationState = IceCandidateNominationState.Nominated;

                _selectedPair = pair;

                TryTransitionTo(IceConnectionState.Connected, expected: IceConnectionStateFlag.Checking);
                CheckCompletion();
            }
        }

        private void TryNominateValidPair()
        {
            lock (_syncLock)
            {
                if (Role != IceRole.Controlling)
                    return;

                if (_validPairs.Any(p => p.NominationState == IceCandidateNominationState.Nominating))
                    return;

                var toNominate = _validPairs
                    .OrderByDescending(p => p.Priority)
                    .FirstOrDefault();

                if (toNominate is not { NominationState: IceCandidateNominationState.None })
                    return;

                toNominate.NominationState = IceCandidateNominationState.Nominating;

                // todo: find the highest priority valid pair and nominate it.
                // if it fails, remove it from validPairs, and try the next one.
                // only nominate one pair at a time.

                throw new NotImplementedException();
            }
        }

        private bool IsConnectivityCheckMaxConcurrencyReached()
        {
            lock (_syncLock)
            {
                return _connectivityCheckTasks.Count >= _config.MaxConcurrentConnectivityChecks;
            }
        }

        private void ScheduleConnectivityCheck(IceCandidatePair pair, ConnectivityCheckReason reason)
        {
            lock (_syncLock)
            {
                pair.State = IceCandidatePairState.InProgress;

                var task = PerformConnectivityCheckAsync(pair, reason, _cts.Token);

                _connectivityCheckTasks.TryAdd(task, null);

                task.ContinueWith(t =>
                {
                    _connectivityCheckTasks.TryRemove(t, out _);
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        private async Task PerformConnectivityCheckAsync(IceCandidatePair pair, ConnectivityCheckReason reason, CancellationToken cancellation)
        {
            var attributes = new List<IStunAttribute>
            {
                new UsernameAttribute($"{_credentials.UFragRemote}:{_credentials.UFragLocal}"),
                new PriorityAttribute(pair.LocalCandidate.Priority),
                Role == IceRole.Controlling ? new IceControllingAttribute(_tieBreaker) : new IceControlledAttribute(_tieBreaker),
            };

            using var request = new StunMessage(
                    StunClass.Request,
                    StunMethods.BindingRequest,
                    attributes)
                .WithMessageIntegrity(_credentials.PwdLocalBytes)
                .WithFingerprint();

            try
            {
                var (response, _) = await pair.SendStunRequestAsync(request, cancellation);
                using var _ = response;

                if (response is not { Class: StunClass.SuccessResponse, Method: StunMethods.BindingRequest })
                {
                    OnCheckFailed(pair, reason);
                    return;
                }

                AdoptPeerReflexiveCandidate(pair, response);

                OnCheckSucceeded(pair, reason);
            }
            catch (Exception ex)
            {
                OnCheckFailed(pair, reason);
            }
        }

        private void AdoptPeerReflexiveCandidate(IceCandidatePair pair, StunMessage bindingResponse)
        {
            throw new NotImplementedException();
        }

        private void OnCheckSucceeded(IceCandidatePair pair, ConnectivityCheckReason reason)
        {
            lock (_syncLock)
            {
                pair.State = IceCandidatePairState.Succeeded;
                _validPairs.Add(pair);

                if (reason == ConnectivityCheckReason.Nomination &&
                    pair.NominationState == IceCandidateNominationState.Nominating)
                {
                    SelectPair(pair);
                }

                CheckCompletion();
            }
        }

        private void OnCheckFailed(IceCandidatePair pair, ConnectivityCheckReason reason)
        {
            lock (_syncLock)
            {
                pair.State = IceCandidatePairState.Failed;
                pair.NominationState = IceCandidateNominationState.None;
                _validPairs.Remove(pair);

                if (_selectedPair == pair)
                {
                    _selectedPair = null;
                    TryTransitionTo(IceConnectionState.Checking, expected: IceConnectionStateFlag.Connected | IceConnectionStateFlag.Completed);
                }

                CheckCompletion();
            }
        }

        private void OnLocalCandidateGathered(object gatherer, LocalIceCandidate? candidate)
        {
            if (ConnectionState is (IceConnectionState.Completed or IceConnectionState.Failed or IceConnectionState.Closed))
                return;

            lock (_syncLock)
            {
                if (candidate == null)
                {
                    _localGatheringComplete = true;
                    CheckCompletion();
                    return;
                }

                if (_checkList.AddLocalCandidate(candidate))
                {
                    candidate.IceEndPoint.SetStunMessageHandler(new StunMessageHandler(this));
                }
            }
        }

        public void AddRemoteCandidate(RemoteIceCandidate? candidate)
        {
            if (ConnectionState is (IceConnectionState.Completed or IceConnectionState.Failed or IceConnectionState.Closed))
                return;

            lock (_syncLock)
            {
                if (candidate == null)
                {
                    _remoteGatheringComplete = true;
                    CheckCompletion();
                    return;
                }

                _checkList.AddRemoteCandidate(candidate);
            }
        }

        private bool TryTransitionTo(IceConnectionState newState, IceConnectionStateFlag? expected = null, IceConnectionStateFlag? excluded = null)
        {
            lock (_syncLock)
            {
                if (ConnectionState == newState)
                    return false;

                if (expected.HasValue && ((int)newState & (int)expected) != 0)
                    return false;

                if (excluded.HasValue && ((int)newState & (int)excluded) == 0)
                    return false;

                ConnectionState = newState;
                OnStateChanged?.Invoke(newState);
                return true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await ErrorBoundary.ExecuteAsync(async () => await StopAsync().ConfigureAwait(false))
                .ConfigureAwait(false);

            _cts.Cancel();

            List<Task?> tasks = [_checkerTask, _keepaliveTask, _nominatingTask, .._connectivityCheckTasks.Keys];
            await ErrorBoundary.ExecuteAsync(async () =>
                await Task.WhenAll(tasks.Where(t => t != null)!).ConfigureAwait(false)).ConfigureAwait(false);

            _cts.Dispose();
        }

        private enum ConnectivityCheckReason
        {
            Check,
            KeepAlive,
            Nomination
        }
    }
}
